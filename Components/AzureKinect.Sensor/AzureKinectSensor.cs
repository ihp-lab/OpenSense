using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Threading;
using Microsoft.Azure.Kinect.Sensor;
using Microsoft.Extensions.Logging;
using Microsoft.Psi;
using Microsoft.Psi.Calibration;
using Microsoft.Psi.Components;
using Image = Microsoft.Psi.Imaging.Image;

namespace OpenSense.Components.AzureKinect.Sensor {

    public sealed class AzureKinectSensor : ISourceComponent, INotifyPropertyChanged, IDisposable {

        private readonly Pipeline _pipeline;

        private readonly Thread _captureThread;

        private readonly Thread _imuThread;

        #region Options
        private int deviceIndex = 0;

        public int DeviceIndex {
            get => deviceIndex;
            set => SetProperty(ref deviceIndex, value);
        }

        private ColorResolution colorResolution = ColorResolution.Off;

        public ColorResolution ColorResolution {
            get => colorResolution;
            set => SetProperty(ref colorResolution, value);
        }

        private ImageFormat colorFormat = ImageFormat.ColorBGRA32;

        public ImageFormat ColorFormat {
            get => colorFormat;
            set => SetProperty(ref colorFormat, value);
        }

        private FPS cameraFrameRate = FPS.FPS30;

        public FPS CameraFrameRate {
            get => cameraFrameRate;
            set => SetProperty(ref cameraFrameRate, value);
        }

        private DepthMode depthMode = DepthMode.Off;

        public DepthMode DepthMode {
            get => depthMode;
            set => SetProperty(ref depthMode, value);
        }

        private TimeSpan depthDelayOffColor = TimeSpan.Zero;

        public TimeSpan DepthDelayOffColor {
            get => depthDelayOffColor;
            set => SetProperty(ref depthDelayOffColor, value);
        }

        private bool synchronizedImagesOnly = true;

        public bool SynchronizedImagesOnly {
            get => synchronizedImagesOnly;
            set => SetProperty(ref synchronizedImagesOnly, value);
        }

        private ThreadPriority captureThreadPriority = ThreadPriority.AboveNormal;

        public ThreadPriority CaptureThreadPriority {
            get => captureThreadPriority;
            set => SetProperty(ref captureThreadPriority, value);
        }

        private TimeSpan captureThreadJoinTimeout = TimeSpan.FromSeconds(1);

        public TimeSpan CaptureThreadJoinTimeout {
            get => captureThreadJoinTimeout;
            set => SetProperty(ref captureThreadJoinTimeout, value);
        }

        private bool enableIMU;

        public bool EnableIMU {
            get => enableIMU;
            set => SetProperty(ref enableIMU, value);
        }

        private ThreadPriority imuThreadPriority = ThreadPriority.AboveNormal;

        public ThreadPriority ImuThreadPriority {
            get => imuThreadPriority;
            set => SetProperty(ref imuThreadPriority, value);
        }

        private TimeSpan imuSampleTimeout = TimeSpan.FromSeconds(1);

        public TimeSpan ImuSampleTimeout {
            get => imuSampleTimeout;
            set => SetProperty(ref imuSampleTimeout, value);
        }

        private TimeSpan imuThreadJoinTimeout = TimeSpan.FromSeconds(1);

        public TimeSpan ImuThreadJoinTimeout {
            get => imuThreadJoinTimeout;
            set => SetProperty(ref imuThreadJoinTimeout, value);
        }

        private WiredSyncMode wiredSyncMode = WiredSyncMode.Standalone;

        public WiredSyncMode WiredSyncMode {
            get => wiredSyncMode;
            set => SetProperty(ref wiredSyncMode, value);
        }

        private TimeSpan suboridinateDelayOffMaster = TimeSpan.Zero;

        public TimeSpan SuboridinateDelayOffMaster {
            get => suboridinateDelayOffMaster;
            set => SetProperty(ref suboridinateDelayOffMaster, value);
        }

        private int exposureTime = 0;

        public int ExposureTime {
            get => exposureTime;
            set => SetProperty(ref exposureTime, value);
        }

        private int brightness = 128;//This is the default value

        public int Brightness {
            get => brightness;
            set => SetProperty(ref brightness, value);
        }

        private int contrast = -1;

        public int Contrast {
            get => contrast;
            set => SetProperty(ref contrast, value);
        }

        private int saturation = -1;

        public int Saturation {
            get => saturation;
            set => SetProperty(ref saturation, value);
        }

        private int sharpness = -1;

        public int Sharpness {
            get => sharpness;
            set => SetProperty(ref sharpness, value);
        }

        private int whiteBalance = 0;

        public int WhiteBalance {
            get => whiteBalance;
            set => SetProperty(ref whiteBalance, value);
        }

        private BacklightCompensation backlightCompensation = BacklightCompensation.Unspecified;

        public BacklightCompensation BacklightCompensation {
            get => backlightCompensation;
            set => SetProperty(ref backlightCompensation, value);
        }

        private int gain = -1;

        public int Gain {
            get => gain;
            set => SetProperty(ref gain, value);
        }

        private PowerlineFrequency powerlineFrequency = PowerlineFrequency.Unspecified;

        public PowerlineFrequency PowerlineFrequency {
            get => powerlineFrequency;
            set => SetProperty(ref powerlineFrequency, value);
        }

        private bool disableStreamingIndicator;

        public bool DisableStreamingIndicator {
            get => disableStreamingIndicator;
            set => SetProperty(ref disableStreamingIndicator, value);
        }

        private ILogger? logger;

        public ILogger? Logger {
            get => logger;
            set => SetProperty(ref logger, value);
        }
        #endregion

        #region Ports
        public Emitter<Calibration> RawCalibrationOut { get; }

        public Emitter<IDepthDeviceCalibrationInfo> CalibrationOut { get; }

        public Emitter<Shared<Capture>> RawCaptureOut { get; }

        public Emitter<Shared<Image>> ColorOut { get; }

        public Emitter<Shared<Image>> DepthOut { get; }

        public Emitter<Shared<Image>> InfraredOut { get; }

        public Emitter<(Shared<Image> Depth, Shared<Image> Infrared)> DepthAndInfraredOut { get; }

        public Emitter<float> CaptureTemperatureOut { get; }

        public Emitter<ImuSample> RawImuOut { get; }//TODO: find out what time is the timestamps related to

        //TODO: add expaneded IMU outputs
        #endregion

        private bool EnableColor => Enum.IsDefined(typeof(ColorResolution), ColorResolution) && ColorResolution != ColorResolution.Off;

        private bool EnableDepthAndIR => Enum.IsDefined(typeof(DepthMode), DepthMode) && DepthMode != DepthMode.Off;

        private Device? device;

        private bool captureEnabled;

        private bool imuEnabled;

        private bool stopRequested;
        
        public AzureKinectSensor(Pipeline pipeline) {
            _pipeline = pipeline;
            _captureThread = new Thread(CaptureThreadRun) {
                Priority = CaptureThreadPriority,
                IsBackground = true,//not blocking the process from exiting
            };
            _imuThread = new Thread(ImuThreadRun) {
                Priority = ImuThreadPriority,
                IsBackground = true,//not blocking the process from exiting
            };

            RawCalibrationOut = pipeline.CreateEmitter<Calibration>(this, nameof(RawCalibrationOut));
            CalibrationOut = pipeline.CreateEmitter<IDepthDeviceCalibrationInfo>(this, nameof(CalibrationOut));
            RawCaptureOut = pipeline.CreateEmitter<Shared<Capture>>(this, nameof(RawCaptureOut));
            ColorOut = pipeline.CreateEmitter<Shared<Image>>(this, nameof(ColorOut));
            DepthOut = pipeline.CreateEmitter<Shared<Image>>(this, nameof(DepthOut));
            InfraredOut = pipeline.CreateEmitter<Shared<Image>>(this, nameof(InfraredOut));
            DepthAndInfraredOut = pipeline.CreateEmitter<(Shared<Image> Depth, Shared<Image> Infrared)>(this, nameof(DepthAndInfraredOut));
            CaptureTemperatureOut = pipeline.CreateEmitter<float>(this, nameof(CaptureTemperatureOut));
            RawImuOut = pipeline.CreateEmitter<ImuSample>(this, nameof(RawImuOut));

            pipeline.PipelineRun += OnPipelineRun;
            pipeline.PipelineCompleted += OnPipelineCompleted;
        }

        #region ISourceComponent
        public void Start(Action<DateTime> notifyCompletionTime) {
            if (!EnableColor && !EnableDepthAndIR && !EnableIMU) {
                Logger?.LogWarning("No data source is enabled.");
            }

            Trace.Assert(!stopRequested);
            notifyCompletionTime(DateTime.MaxValue);//infinite source

            Trace.Assert(device is null);
            device = Device.Open(DeviceIndex);

            switch (WiredSyncMode) {
                case WiredSyncMode.Standalone:
                    break;
                case WiredSyncMode.Master:
                    /* NOTE:
                     * The 'Sync Out' jack is enabled and synchronization data it driven out the connected wire.
                     * While in master mode the color camera must be enabled as part of the multi device sync signalling logic. Even if the color image is not needed, the color camera must be running.
                     */
                    if (!EnableColor) {
                        throw new Exception("The Master mode requires the color camera to be enabled.");
                    }
                    if (!device.SyncOutJackConnected) {
                        throw new Exception("Cannot set Sensor as Master if Sync-Out Jack is not connected");
                    }
                    break;
                case WiredSyncMode.Subordinate:
                    /* NOTE:
                     * The 'Sync In' jack is used for synchronization and 'Sync Out' is driven for the next device in the chain.
                     * 'Sync Out' is a mirror of 'Sync In' for this mode.
                     * */
                    if (!device.SyncInJackConnected) {
                        throw new Exception("Cannot set Sensor as Subordinate if Sync-In Jack is not connected");
                    }
                    break;
                default:
                    throw new InvalidEnumArgumentException(nameof(WiredSyncMode), (int)WiredSyncMode, typeof(WiredSyncMode));
            }

            if (EnableColor || EnableDepthAndIR) {
                Trace.Assert(!captureEnabled);
                captureEnabled = true;

                #region Color Control Commands
                /* Color Control Commands */
                /* NOTE:
                 * 
                 * k4a_device_set_color_control():
                 * https://microsoft.github.io/Azure-Kinect-Sensor-SDK/master/group___functions_gae81269489170b26b7f0bbe1a7f9d31d6.html
                 * Control values set on a device are reset only when the device is power cycled. The device will retain the settings even if the k4a_device_t is closed or the application is restarted.
                 * 
                 * k4a_color_control_command_t:
                 * https://microsoft.github.io/Azure-Kinect-Sensor-SDK/master/group___enumerations_gafba23de13b10b11e413485e421aa0468.html
                 */
                if (ExposureTime == 0) {
                    device.SetColorControl(ColorControlCommand.ExposureTimeAbsolute, ColorControlMode.Auto, 0/* value ignored */);
                } else if (ExposureTime > 0) {
                    device.SetColorControl(ColorControlCommand.ExposureTimeAbsolute, ColorControlMode.Manual, ExposureTime);
                } else {
                    //Use the setting from last time.
                }
                if (Brightness >= 0) {
                    if (Brightness > 255) {
                        throw new Exception("Brightness must be less than or equal to 255.");
                    }
                    device.SetColorControl(ColorControlCommand.Brightness, ColorControlMode.Manual, Brightness);
                }
                if (Contrast >= 0) {
                    device.SetColorControl(ColorControlCommand.Contrast, ColorControlMode.Manual, Contrast);
                }
                if (Saturation >= 0) {
                    device.SetColorControl(ColorControlCommand.Saturation, ColorControlMode.Manual, Saturation);
                }
                if (Sharpness >= 0) {
                    device.SetColorControl(ColorControlCommand.Sharpness, ColorControlMode.Manual, Sharpness);
                }
                if (WhiteBalance == 0) {
                    device.SetColorControl(ColorControlCommand.Whitebalance, ColorControlMode.Auto, 0/* value ignored */);
                } else if (WhiteBalance > 0) {
                    if (WhiteBalance % 10 != 0) {
                        throw new Exception("Whitebalance must be a multiple of 10.");
                    }
                    device.SetColorControl(ColorControlCommand.Whitebalance, ColorControlMode.Manual, WhiteBalance);
                }
                switch (BacklightCompensation) {
                    case BacklightCompensation.Unspecified:
                        break;
                    case BacklightCompensation.Disabled:
                        device.SetColorControl(ColorControlCommand.BacklightCompensation, ColorControlMode.Manual, 0);
                        break;
                    case BacklightCompensation.Enabled:
                        device.SetColorControl(ColorControlCommand.BacklightCompensation, ColorControlMode.Manual, 1);
                        break;
                    default:
                        throw new InvalidEnumArgumentException(nameof(BacklightCompensation), (int)BacklightCompensation, typeof(BacklightCompensation));
                }
                if (Gain >= 0) {
                    device.SetColorControl(ColorControlCommand.Gain, ColorControlMode.Manual, Gain);
                }
                switch (PowerlineFrequency) {
                    case PowerlineFrequency.Unspecified:
                        break;
                    case PowerlineFrequency.FiftyHertz:
                        device.SetColorControl(ColorControlCommand.PowerlineFrequency, ColorControlMode.Manual, 50);
                        break;
                    case PowerlineFrequency.SixtyHertz:
                        device.SetColorControl(ColorControlCommand.PowerlineFrequency, ColorControlMode.Manual, 60);
                        break;
                    default:
                        throw new InvalidEnumArgumentException(nameof(PowerlineFrequency), (int)PowerlineFrequency, typeof(PowerlineFrequency));
                }
                #endregion

                var deviceConfiguration = new DeviceConfiguration {
                    ColorFormat = ColorFormat,
                    ColorResolution = ColorResolution,
                    DepthMode = DepthMode,
                    CameraFPS = CameraFrameRate,
                    SynchronizedImagesOnly = SynchronizedImagesOnly,
                    DepthDelayOffColor = DepthDelayOffColor,
                    WiredSyncMode = WiredSyncMode,
                    SuboridinateDelayOffMaster = SuboridinateDelayOffMaster,
                    DisableStreamingIndicator = DisableStreamingIndicator,
                };
                device.StartCameras(deviceConfiguration);
                _captureThread.Start();
            }

            //TODO: test if it is possible to only capture IMU without color, depth, and ir
            if (EnableIMU) {
                Trace.Assert(!imuEnabled);
                imuEnabled = true;
                device.StartImu();
                _imuThread.Start();
            }
        }

        public void Stop(DateTime finalOriginatingTime, Action notifyCompleted) {
            Trace.Assert(!stopRequested);
            EnsureStop();
            notifyCompleted();
        }
        #endregion

        #region Pipeline Events
        private void OnPipelineRun(object? sender, PipelineRunEventArgs e) {
            var canGetCalibration = EnableColor && EnableDepthAndIR;
            if (!canGetCalibration) {
                return;
            }
            //TODO: output k4a calibration and psi calibration
        }

        private void OnPipelineCompleted(object? sender, PipelineCompletedEventArgs e) {
            EnsureStop();
        }
        #endregion

        #region Threads
        private void CaptureThreadRun() {
            //TODO: output color, depth, and ir
        }

        private void ImuThreadRun() {
            Trace.Assert(device is not null);
            while (!stopRequested) {
                var sample = device!.GetImuSample(ImuSampleTimeout);
                var timestamp = _pipeline.GetCurrentTime();
                RawImuOut.Post(sample, timestamp);
            }
        }
        #endregion

        private void EnsureStop() {
            if (stopRequested) {
                return;
            }
            stopRequested = true;

            Trace.Assert(device is not null);
            if (captureEnabled) {
                if (!_captureThread.Join(CaptureThreadJoinTimeout)) {
                    _captureThread.Abort();
                }
                device!.StopCameras();
            }

            if (imuEnabled) {
                if (!_imuThread.Join(ImuThreadJoinTimeout)) {
                    _imuThread.Abort();
                }
                device!.StopImu();
            }

            device!.Dispose();//dispose early
            device = null;
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

        #region IDisposable
        private bool disposed;

        public void Dispose() {
            if (disposed) {
                return;
            }
            disposed = true;

            EnsureStop();
        }
        #endregion
    }
}
