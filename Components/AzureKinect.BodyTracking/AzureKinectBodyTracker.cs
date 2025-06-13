using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Microsoft.Azure.Kinect.BodyTracking;
using Microsoft.Azure.Kinect.Sensor;
using Microsoft.Extensions.Logging;
using Microsoft.Psi;

namespace OpenSense.Components.AzureKinect.BodyTracking {
    public sealed class AzureKinectBodyTracker : INotifyPropertyChanged, IDisposable {

        private const string K4abtLibDir = "k4abt";//Set in csproj

        private const string K4abtDll = "k4abt.dll";

        #region Options

        private SensorOrientation sensorOrientation;

        public SensorOrientation SensorOrientation {
            get => sensorOrientation;
            set => SetProperty(ref sensorOrientation, value);
        }

        private TrackerProcessingMode processingBackend;

        public TrackerProcessingMode ProcessingBackend {
            get => processingBackend;
            set => SetProperty(ref processingBackend, value);
        }

        private int gpuDeviceId;

        public int GpuDeviceId {
            get => gpuDeviceId;
            set => SetProperty(ref gpuDeviceId, value);
        }

        private bool useLiteModel;

        public bool UseLiteModel {
            get => useLiteModel;
            set => SetProperty(ref useLiteModel, value);
        }

        private float temporalSmoothing;

        public float TemporalSmoothing {
            get => temporalSmoothing;
            set => SetProperty(ref temporalSmoothing, value);
        }

        private TimeSpan timeout = TimeSpan.FromSeconds(-1);

        public TimeSpan Timeout {
            get => timeout;
            set => SetProperty(ref timeout, value);
        }

        private bool throwOnTimeout;

        public bool ThrowOnTimeout {
            get => throwOnTimeout;
            set => SetProperty(ref throwOnTimeout, value);
        }

        private bool outputNull;

        public bool OutputNull {
            get => outputNull;
            set => SetProperty(ref outputNull, value);
        }

        private ILogger? logger;

        public ILogger? Logger {
            get => logger;
            set => SetProperty(ref logger, value);
        }
        #endregion

        #region Ports
        public Receiver<Calibration> CalibrationIn { get; }

        public Receiver<Shared<Capture>> CaptureIn { get; }

        public Emitter<Shared<Frame?>> FrameOut { get; }

        public Emitter<IReadOnlyList<Body>?> BodiesOut { get; }
        #endregion

        /// <remarks>According to the documentation of k4abt_tracker_create(), only one tracker is allowed to exist at the same time in each process.</remarks>
        private Tracker? tracker;

        public AzureKinectBodyTracker(Pipeline pipeline) {
            CalibrationIn = pipeline.CreateReceiver<Calibration>(this, ProcessCalibration, nameof(CalibrationIn));
            CaptureIn = pipeline.CreateReceiver<Shared<Capture>>(this, ProcessCapture, nameof(CaptureIn));

            FrameOut = pipeline.CreateEmitter<Shared<Frame?>>(this, nameof(FrameOut));
            BodiesOut = pipeline.CreateEmitter<IReadOnlyList<Body>?>(this, nameof(BodiesOut));

            pipeline.PipelineRun += OnPipelineRun;
            pipeline.PipelineCompleted += OnPipelineCompleted;
        }

        #region Pipeline Events
        private void OnPipelineRun(object? sender, PipelineRunEventArgs args) {
            
        }

        private void OnPipelineCompleted(object? sender, PipelineCompletedEventArgs args) {
            Dispose();
        }
        #endregion

        private void ProcessCalibration(Calibration calibration, Envelope envelope) {
            Trace.Assert(tracker is null);
            Trace.Assert(0 <= TemporalSmoothing && TemporalSmoothing <= 1);
            var modelPath = UseLiteModel ? "dnn_model_2_0_lite_op11.onnx" : "dnn_model_2_0_op11.onnx";
            var trackerConfiguration = new TrackerConfiguration() {
                SensorOrientation = SensorOrientation,
                ProcessingMode = ProcessingBackend,
                GpuDeviceId = GpuDeviceId,
                ModelPath = modelPath,
            };
            PreloadK4abtAndDependencies();
            /** NOTE:
             * AzureKinectBodyTrackingCreateException does not contain any error detail.
             * To know the detail, you need to look at its log.
             * k4abt.dll does not expose APIs like k4a.dll does for tracking logs within the program.
             * To view the logs, there are two options: console output or logging to a file.
             * By default, logs are output to the console.
             * If the component is launched by the WPF Applications, temporarily changing the OutputType from WinExe to Exe will display the terminal.
             * If the K4ABT_ENABLE_LOG_TO_A_FILE environment variable is set, logs will be written to a file.
             * The file path must end with .log to be valid.
             * https://learn.microsoft.com/en-us/previous-versions/azure/kinect-dk/troubleshooting#collecting-logs
             *
             * One failure I have encountered was due to ONNX runtime not compatible.
             */
            var tkr = Tracker.Create(calibration, trackerConfiguration);
            tkr.SetTemporalSmooting(TemporalSmoothing);
            tracker = tkr;//Post tracker instance
            Logger?.LogDebug("Tracker initialized.");
        }

        private void ProcessCapture(Shared<Capture> capture, Envelope envelope) {
            if (tracker is null) {
                Logger?.LogDebug("Calibration has not been received, skipping the capture.");
                return;
            }
            var cap = capture.Resource;
            if (cap.IR is null) {
                Logger?.LogDebug("IR image is null, skipping the capture.");
                return;
            }
            if (cap.Depth is null) {
                Logger?.LogDebug("Depth image is null, skipping the capture.");
                return;
            }
            Trace.Assert(cap.IR.Format == ImageFormat.IR16);
            Trace.Assert(cap.Depth.Format == ImageFormat.Depth16);
            tracker.EnqueueCapture(cap);
            var frame = (Frame?)tracker.PopResult(Timeout, ThrowOnTimeout);
            using var sharedFrame = Shared.Create(frame);
            if (frame is null) {
                if (OutputNull) {
                    FrameOut.Post(sharedFrame, envelope.OriginatingTime);
                    if (BodiesOut.HasSubscribers) {
                        BodiesOut.Post(null, envelope.OriginatingTime);
                    }
                }
                return;
            }
            FrameOut.Post(sharedFrame, envelope.OriginatingTime);
            if (BodiesOut.HasSubscribers) {
                var numBodies = frame.NumberOfBodies;
                Body[] bodies;
                if (numBodies == 0) {
                    bodies = Array.Empty<Body>();
                } else {
                    bodies = new Body[numBodies];
                    for (var i = 0u; i < numBodies; i++) {
                        var body = new Body(i, frame.GetBody(i));
                        bodies[i] = body;
                    }
                }
                BodiesOut.Post(bodies, envelope.OriginatingTime);
            }
        }

        #region INotifyPropertyChanged
        public event PropertyChangedEventHandler? PropertyChanged;

        private void SetProperty<T>(ref T field, T value, [CallerMemberName] string? propertyName = null) {
            if (!EqualityComparer<T>.Default.Equals(field, value)) {
                field = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            }
        }
        #endregion

        #region P/Invoke

        [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        private static extern IntPtr LoadLibraryEx(string lpFileName, IntPtr hFile, uint dwFlags);

        public static void PreloadK4abtAndDependencies() {
            var baseDir = AppContext.BaseDirectory;
            if (File.Exists(Path.Combine(baseDir, K4abtDll))) {
                /* NOTE:
                 * Since the dependencies of k4abt.dll are located in both the application base directory and the AzureKinectLibs directory (LOAD_LIBRARY_SEARCH_USER_DIRS),
                 * we need to enable both locations in the DLL search path.
                 *
                 * However, Windows only supports enabling or disabling directories for DLL searches—it does not allow us to explicitly define the search order between them.
                 *
                 * This means that if k4abt.dll exists in the application base directory, its dependencies (ONNX DLLs) will be loaded from the base directory, even if we explicitly load k4abt.dll from AzureKinectLibs.
                 * This behavior is counterintuitive, but observed.
                 *
                 * As a result, we ensure that k4abt.dll does not exist in the application base directory.
                 */
                throw new Exception($"k4abt.dll exists in application base directory. It shouldn't be there.");
            }
            var extraDllDir = Path.Combine(baseDir, K4abtLibDir);
            var k4abtPath = Path.Combine(extraDllDir, K4abtDll);
            var handle = LoadLibraryEx(k4abtPath, IntPtr.Zero, 0);//We have to load this k4abt.dll, otherwise there is no chance our application knows it is there.
            if (handle == IntPtr.Zero) {
                int error = Marshal.GetLastWin32Error();
                throw new InvalidOperationException($"Failed to load k4abt.dll, error code: {error}");
            }
        }
        #endregion

        #region IDisposable
        private bool disposed;

        public void Dispose() {
            if (disposed) {
                return;
            }
            disposed = true;

            if (tracker is not null) {
                tracker.Shutdown();
                tracker.Dispose();
                tracker = null;
            }
        }
        #endregion
    }
}
