using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Double;
using MathNet.Spatial.Euclidean;
using Microsoft.Azure.Kinect.Sensor;
using Microsoft.Extensions.Logging;
using Microsoft.Psi;
using Microsoft.Psi.Calibration;
using Microsoft.Psi.Components;
using Microsoft.Psi.Imaging;
using Image = Microsoft.Psi.Imaging.Image;
using KinectImage = Microsoft.Azure.Kinect.Sensor.Image;

namespace OpenSense.Components.AzureKinect.Sensor {
    /// <remarks>This component's code is based on \psi's code.</remarks>
    public sealed class AzureKinectSensor : ISourceComponent, INotifyPropertyChanged, IDisposable {

        private const PixelFormat ColorImageFormat = PixelFormat.BGRA_32bpp;

        private const PixelFormat InfraredImageFormat = PixelFormat.Gray_16bpp;

        private const double DepthValueToMetersScaleFactor = 0.001;

        private readonly Pipeline _pipeline;

        private readonly Thread _captureThread;

        private readonly Thread _imuThread;

        #region Options
        private int deviceIndex = 0;

        public int DeviceIndex {
            get => deviceIndex;
            set => SetProperty(ref deviceIndex, value);
        }

        private ColorResolution colorResolution = ColorResolution.R2160p;

        public ColorResolution ColorResolution {
            get => colorResolution;
            set => SetProperty(ref colorResolution, value);
        }

        private ImageFormat colorFormat = ImageFormat.ColorBGRA32;

        public ImageFormat ColorFormat {
            get => colorFormat;
            set => SetProperty(ref colorFormat, value);
        }

        private FPS frameRate = FPS.FPS30;

        public FPS FrameRate {
            get => frameRate;
            set => SetProperty(ref frameRate, value);
        }

        private DepthMode depthMode = DepthMode.WFOV_2x2Binned;

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

        private TimeSpan captureSampleTimeout = TimeSpan.FromSeconds(1);

        public TimeSpan CaptureSampleTimeout {
            get => captureSampleTimeout;
            set => SetProperty(ref captureSampleTimeout, value);
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

        private bool streamingIndicator = true;

        public bool StreamingIndicator {
            get => streamingIndicator;
            set => SetProperty(ref streamingIndicator, value);
        }

        private ILogger? logger;

        public ILogger? Logger {
            get => logger;
            set => SetProperty(ref logger, value);
        }
        #endregion

        #region Ports
        public Emitter<Calibration> CalibrationOut { get; }

        public Emitter<IDepthDeviceCalibrationInfo> PsiCalibrationOut { get; }

        public Emitter<Shared<Capture>> CaptureOut { get; }

        public Emitter<Shared<KinectImage>> ColorOut { get; }

        public Emitter<Shared<KinectImage>> DepthOut { get; }

        public Emitter<Shared<KinectImage>> InfraredOut { get; }

        public Emitter<Shared<Image>> PsiColorOut { get; }

        public Emitter<Shared<DepthImage>> PsiDepthOut { get; }

        public Emitter<Shared<Image>> PsiInfraredOut { get; }

        public Emitter<float> CaptureTemperatureOut { get; }

        public Emitter<ImuSample> ImuOut { get; }

        public Emitter<float> ImuTemperatureOut { get; }
        #endregion

        #region Public Properties
        public Calibration? RawCalibration { get; private set; }

        public IDepthDeviceCalibrationInfo? Calibration { get; private set; } 

        public bool? SyncInJackConnected => device?.SyncInJackConnected;

        public bool? SyncOutJackConnected => device?.SyncOutJackConnected;
        #endregion

        private bool EnableColor => Enum.IsDefined(typeof(ColorResolution), ColorResolution) && ColorResolution != ColorResolution.Off;

        private bool EnableDepth => Enum.IsDefined(typeof(DepthMode), DepthMode) && DepthMode != DepthMode.Off;

        private Device? device;

        private bool captureEnabled;

        private bool imuEnabled;

        private bool stopRequested;
        
        public AzureKinectSensor(Pipeline pipeline) {
            _pipeline = pipeline;
            _captureThread = new Thread(CaptureThreadRun) {
                Name = "Azure Kinect Capture Thread",
                Priority = CaptureThreadPriority,
                IsBackground = true,//not blocking the process from exiting
            };
            _imuThread = new Thread(ImuThreadRun) {
                Name = "Azure Kinect IMU Thread",
                Priority = ImuThreadPriority,
                IsBackground = true,//not blocking the process from exiting
            };

            CalibrationOut = pipeline.CreateEmitter<Calibration>(this, nameof(CalibrationOut));
            PsiCalibrationOut = pipeline.CreateEmitter<IDepthDeviceCalibrationInfo>(this, nameof(PsiCalibrationOut));
            CaptureOut = pipeline.CreateEmitter<Shared<Capture>>(this, nameof(CaptureOut));
            ColorOut = pipeline.CreateEmitter<Shared<KinectImage>>(this, nameof(ColorOut));
            DepthOut = pipeline.CreateEmitter<Shared<KinectImage>>(this, nameof(DepthOut));
            InfraredOut = pipeline.CreateEmitter<Shared<KinectImage>>(this, nameof(InfraredOut));
            PsiColorOut = pipeline.CreateEmitter<Shared<Image>>(this, nameof(PsiColorOut));
            PsiDepthOut = pipeline.CreateEmitter<Shared<DepthImage>>(this, nameof(PsiDepthOut));
            PsiInfraredOut = pipeline.CreateEmitter<Shared<Image>>(this, nameof(PsiInfraredOut));
            CaptureTemperatureOut = pipeline.CreateEmitter<float>(this, nameof(CaptureTemperatureOut));
            ImuOut = pipeline.CreateEmitter<ImuSample>(this, nameof(ImuOut));
            ImuTemperatureOut = pipeline.CreateEmitter<float>(this, nameof(ImuTemperatureOut));

            pipeline.PipelineRun += OnPipelineRun;
            pipeline.PipelineCompleted += OnPipelineCompleted;
        }

        #region ISourceComponent
        public void Start(Action<DateTime> notifyCompletionTime) {//I found the execution order of Start() and OnPipelineRun() is non-deterministic.
            Trace.Assert(!stopRequested);
            notifyCompletionTime(DateTime.MaxValue);//infinite source
        }

        public void Stop(DateTime finalOriginatingTime, Action notifyCompleted) {
            Trace.Assert(!stopRequested);
            EnsureStop();
            notifyCompleted();
        }
        #endregion

        #region Pipeline Events
        private void OnPipelineRun(object? sender, PipelineRunEventArgs e) {//I found the execution order of Start() and OnPipelineRun() is non-deterministic.
            if (!EnableColor && !EnableDepth) {
                throw new InvalidOperationException("Neither the color camera nor the depth camera is enabled in the configuration. At least one must be enabled, even if you don't plan to use them.");
            }
            if (SynchronizedImagesOnly && (!EnableColor || !EnableDepth)) {
                throw new InvalidOperationException("To enable Synchronized Images Only, both depth camera and color camera must also be enabled.");
            }
            if (ColorResolution == ColorResolution.R3072p && FrameRate == FPS.FPS30) {
                throw new InvalidOperationException("The combination of Color Resolution at 3072P, and Frame Rate at 30 is not supported.");
            }
            if (DepthMode == DepthMode.WFOV_Unbinned && FrameRate == FPS.FPS30) {
                throw new InvalidOperationException("The combination of Depth Mode at WFOV Unbinned, and Frame Rate at 30 is not supported.");
            }

            try {
                /* Launch */
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

                Trace.Assert(!captureEnabled);
                captureEnabled = true;
                var deviceConfiguration = new DeviceConfiguration {
                    ColorFormat = ColorFormat,
                    ColorResolution = ColorResolution,
                    DepthMode = DepthMode,
                    CameraFPS = FrameRate,
                    SynchronizedImagesOnly = SynchronizedImagesOnly,
                    DepthDelayOffColor = DepthDelayOffColor,
                    WiredSyncMode = WiredSyncMode,
                    SuboridinateDelayOffMaster = SuboridinateDelayOffMaster,
                    DisableStreamingIndicator = !StreamingIndicator,
                };
                device.StartCameras(deviceConfiguration);//Color or depth, at least one need to be enabled.
                _captureThread.Name = $"Azure Kinect ({device.SerialNum}) Capture Thread";
                _captureThread.Start();

                if (EnableIMU) {
                    Trace.Assert(!imuEnabled);
                    imuEnabled = true;
                    device.StartImu();//Cameras must has been started.
                    _imuThread.Name = $"Azure Kinect ({device.SerialNum}) IMU Thread";
                    _imuThread.Start();
                }

                /* Raw Calibration */
                Trace.Assert(device is not null);
                var rawCalibration = device!.GetCalibration();
                if (rawCalibration == default) {
                    throw new InvalidOperationException($"Could not get camera calibration.");
                }
                RawCalibration = rawCalibration;
                CalibrationOut.Post(rawCalibration, e.StartOriginatingTime);

                /* Psi Calibration */
                var colorExtrinsics = rawCalibration.ColorCameraCalibration.Extrinsics;
                var colorIntrinsics = rawCalibration.ColorCameraCalibration.Intrinsics;
                var depthIntrinsics = rawCalibration.DepthCameraCalibration.Intrinsics;
                if (colorIntrinsics.Type == CalibrationModelType.Rational6KT || depthIntrinsics.Type == CalibrationModelType.Rational6KT) {
                    throw new InvalidOperationException("Calibration output not permitted for deprecated internal Azure Kinect cameras. Only Brown_Conrady calibration supported.");
                }
                if (colorIntrinsics.Type != CalibrationModelType.BrownConrady || depthIntrinsics.Type != CalibrationModelType.BrownConrady) {
                    throw new InvalidOperationException("Calibration output only supported for Brown_Conrady model.");
                }
                var colorCameraMatrix = Matrix<double>.Build.Dense(3, 3);
                colorCameraMatrix[0, 0] = colorIntrinsics.Parameters[2];
                colorCameraMatrix[1, 1] = colorIntrinsics.Parameters[3];
                colorCameraMatrix[0, 2] = colorIntrinsics.Parameters[0];
                colorCameraMatrix[1, 2] = colorIntrinsics.Parameters[1];
                colorCameraMatrix[2, 2] = 1;
                var depthCameraMatrix = Matrix<double>.Build.Dense(3, 3);
                depthCameraMatrix[0, 0] = depthIntrinsics.Parameters[2];
                depthCameraMatrix[1, 1] = depthIntrinsics.Parameters[3];
                depthCameraMatrix[0, 2] = depthIntrinsics.Parameters[0];
                depthCameraMatrix[1, 2] = depthIntrinsics.Parameters[1];
                depthCameraMatrix[2, 2] = 1;
                var depthToColorMatrix = Matrix<double>.Build.Dense(4, 4);
                for (var i = 0; i < 3; i++) {
                    for (var j = 0; j < 3; j++) {
                        // The AzureKinect SDK assumes that vectors are row vectors, while the MathNet SDK assumes column vectors, so we need to flip them here.
                        depthToColorMatrix[i, j] = colorExtrinsics.Rotation[(j * 3) + i];
                    }
                }

                depthToColorMatrix[3, 0] = colorExtrinsics.Translation[0];
                depthToColorMatrix[3, 1] = colorExtrinsics.Translation[1];
                depthToColorMatrix[3, 2] = colorExtrinsics.Translation[2];
                depthToColorMatrix[3, 3] = 1.0;
                var metersToMillimeters = Matrix<double>.Build.Dense(4, 4);
                metersToMillimeters[0, 0] = 1000.0;
                metersToMillimeters[1, 1] = 1000.0;
                metersToMillimeters[2, 2] = 1000.0;
                metersToMillimeters[3, 3] = 1.0;
                var millimetersToMeters = Matrix<double>.Build.Dense(4, 4);
                millimetersToMeters[0, 0] = 1.0 / 1000.0;
                millimetersToMeters[1, 1] = 1.0 / 1000.0;
                millimetersToMeters[2, 2] = 1.0 / 1000.0;
                millimetersToMeters[3, 3] = 1.0;
                depthToColorMatrix = (metersToMillimeters * depthToColorMatrix * millimetersToMeters).Transpose();

                var colorRadialDistortion = new double[6] {
                    colorIntrinsics.Parameters[4],
                    colorIntrinsics.Parameters[5],
                    colorIntrinsics.Parameters[6],
                    colorIntrinsics.Parameters[7],
                    colorIntrinsics.Parameters[8],
                    colorIntrinsics.Parameters[9],
                };
                var colorTangentialDistortion = new double[2] {
                    colorIntrinsics.Parameters[13],
                    colorIntrinsics.Parameters[12]
                };
                var depthRadialDistortion = new double[6] {
                    depthIntrinsics.Parameters[4],
                    depthIntrinsics.Parameters[5],
                    depthIntrinsics.Parameters[6],
                    depthIntrinsics.Parameters[7],
                    depthIntrinsics.Parameters[8],
                    depthIntrinsics.Parameters[9],
                };
                var depthTangentialDistortion = new double[2] {
                    depthIntrinsics.Parameters[13],
                    depthIntrinsics.Parameters[12]
                };
                var kinectBasis = new CoordinateSystem(default, UnitVector3D.ZAxis, UnitVector3D.XAxis.Negate(), UnitVector3D.YAxis.Negate());// Azure Kinect uses a basis under the hood that assumes Forward=Z, Right=X, Down=Y.
                var psiCalibration = new DepthDeviceCalibrationInfo(
                    rawCalibration.ColorCameraCalibration.ResolutionWidth,
                    rawCalibration.ColorCameraCalibration.ResolutionHeight,
                    colorCameraMatrix,
                    colorRadialDistortion,
                    colorTangentialDistortion,
                    kinectBasis.Invert() * depthToColorMatrix * kinectBasis,
                    rawCalibration.DepthCameraCalibration.ResolutionWidth,
                    rawCalibration.DepthCameraCalibration.ResolutionHeight,
                    depthCameraMatrix,
                    depthRadialDistortion,
                    depthTangentialDistortion,
                    DenseMatrix.CreateIdentity(4)
                );
                Calibration = psiCalibration;
                PsiCalibrationOut.Post(psiCalibration, e.StartOriginatingTime);
            } catch (AzureKinectException ex) {
                LogAzureKinectExceptionDetail(ex);
                throw;
            }
        }

        private void OnPipelineCompleted(object? sender, PipelineCompletedEventArgs e) {
            EnsureStop();
        }
        #endregion

        #region Threads
        private void CaptureThreadRun() {
            Trace.Assert(device is not null);
            /** NOTE:
             * Not using Image.WidthPixels and Image.HeightPixels properties, because they invoke native methods.
             */
            var (colorWidth, colorHeight) = ColorResolution switch {
                ColorResolution.Off => (0, 0),
                ColorResolution.R720p => (1280, 720),
                ColorResolution.R1080p => (1920, 1080),
                ColorResolution.R1440p => (2560, 1440),
                ColorResolution.R1536p => (2048, 1536),
                ColorResolution.R2160p => (3840, 2160),
                ColorResolution.R3072p => (4096, 3072),
                _ => throw new InvalidEnumArgumentException(nameof(ColorResolution), (int)ColorResolution, typeof(ColorResolution)),
            };
            var (depthWidth, depthHeight) = DepthMode switch {
                DepthMode.Off => (0, 0),
                DepthMode.NFOV_2x2Binned => (320, 288),
                DepthMode.NFOV_Unbinned => (640, 576),
                DepthMode.WFOV_2x2Binned => (512, 512),
                DepthMode.WFOV_Unbinned => (1024, 1024),
                DepthMode.PassiveIR => (1024, 1024),
                _ => throw new InvalidEnumArgumentException(nameof(DepthMode), (int)DepthMode, typeof(DepthMode)),
            };
            /** NOTE:
             * We only post \psi data types if there are subscribers to achieve the optimal performance.
             */
            var colorOutHasSubscribers = ColorOut.HasSubscribers;
            var infraredOutHasSubscribers = InfraredOut.HasSubscribers;
            var depthOutHasSubscribers = DepthOut.HasSubscribers;
            var psiColorOutHasSubscribers = PsiColorOut.HasSubscribers;
            var psiInfraredOutHasSubscribers = PsiInfraredOut.HasSubscribers;
            var psiDepthOutHasSubscribers = PsiDepthOut.HasSubscribers;
            try {
                try {
                    while (!stopRequested) {
                        Capture capture;
                        try {
                            capture = device!.GetCapture(CaptureSampleTimeout);
                        } catch (TimeoutException) {
                            Logger?.LogDebug("Timeout while getting a capture sample.");
                            continue;
                        }
                        using var sharedCapture = Shared.Create(capture);
                        var timestamp = _pipeline.GetCurrentTime();
                        CaptureOut.Post(sharedCapture, timestamp);

                        /* Temperature */
                        CaptureTemperatureOut.Post(capture.Temperature, timestamp);

                        /* Color */
                        var cFlag = colorOutHasSubscribers || psiColorOutHasSubscribers;
                        if (cFlag && EnableColor) {
                            if (capture.Color is null) {
                                Logger?.LogDebug("Color image is not available.");
                            } else {
                                if (colorOutHasSubscribers) {
                                    var dup = capture.Color.Reference();
                                    using var shared = Shared.Create(dup);
                                    ColorOut.Post(shared, timestamp);
                                }
                                if (psiColorOutHasSubscribers) {
                                    if (ColorFormat == ImageFormat.ColorBGRA32) {
                                        using var shared = ImagePool.GetOrCreate(colorWidth, colorHeight, ColorImageFormat);
                                        unsafe {
                                            var ptr = GetAzureKinectImageBufferPtr(capture.Color);
                                            var length = checked((int)capture.Color.Size);
                                            shared.Resource.CopyFrom(ptr, length);
                                        }
                                        PsiColorOut.Post(shared, timestamp);
                                    }
                                }
                            }
                        }

                        /* Infrared and Depth */
                        var iFlag = infraredOutHasSubscribers || psiInfraredOutHasSubscribers;
                        var dFlag = depthOutHasSubscribers || psiDepthOutHasSubscribers;
                        var flag = iFlag || dFlag;
                        if (flag && EnableDepth) {
                            /* IR */
                            if (iFlag) {
                                if (capture.IR is null) {
                                    Logger?.LogDebug("Infrared image is not available.");
                                } else {
                                    if (infraredOutHasSubscribers) {
                                        var dup = capture.IR.Reference();
                                        using var shared = Shared.Create(dup);
                                        InfraredOut.Post(shared, timestamp);
                                    }
                                    if (psiInfraredOutHasSubscribers) {
                                        using var shared = ImagePool.GetOrCreate(depthWidth, depthHeight, InfraredImageFormat);
                                        unsafe {
                                            var ptr = GetAzureKinectImageBufferPtr(capture.IR);
                                            var length = checked((int)capture.IR.Size);
                                            shared.Resource.CopyFrom(ptr, length);
                                        }
                                        PsiInfraredOut.Post(shared, timestamp);
                                    }
                                }
                            }

                            /* Depth */
                            if (dFlag && DepthMode != DepthMode.PassiveIR) {
                                if (capture.Depth is null) {
                                    Logger?.LogDebug("Depth image is not available.");
                                } else {
                                    if (depthOutHasSubscribers) {
                                        var dup = capture.Depth.Reference();
                                        using var shared = Shared.Create(dup);
                                        DepthOut.Post(shared, timestamp);
                                    }
                                    if (psiDepthOutHasSubscribers) {
                                        using var shared = DepthImagePool.GetOrCreate(depthWidth, depthHeight, DepthValueSemantics.DistanceToPlane, DepthValueToMetersScaleFactor);
                                        unsafe {
                                            var ptr = GetAzureKinectImageBufferPtr(capture.Depth);
                                            var length = checked((int)capture.Depth.Size);
                                            shared.Resource.UnmanagedBuffer.CopyFrom(ptr, length);//DepthImage does not have a CopyFrom(IntPtr, int) method.
                                        }
                                        PsiDepthOut.Post(shared, timestamp);
                                    }
                                }
                            }
                        }
                    }
                } catch (AzureKinectException ex) {
                    LogAzureKinectExceptionDetail(ex);
                    throw;
                }
            } catch (Exception ex) {
                Logger?.LogError(ex, "Error occurred in the Azure Kinect ({serialNumber}) capture thread.", device!.SerialNum);
            }
        }

        private void ImuThreadRun() {
            Trace.Assert(device is not null);
            try {
                try {
                    while (!stopRequested) {
                        ImuSample sample;
                        try {
                            sample = device!.GetImuSample(ImuSampleTimeout);
                        } catch (TimeoutException) {
                            Logger?.LogDebug("Timeout while getting an IMU sample.");
                            continue;
                        }
                        var timestamp = _pipeline.GetCurrentTime();
                        ImuOut.Post(sample, timestamp);
                        ImuTemperatureOut.Post(sample.Temperature, timestamp);
                    }
                } catch (AzureKinectException ex) {
                    LogAzureKinectExceptionDetail(ex);
                    throw;
                }
            } catch (Exception ex) {
                Logger?.LogError(ex, "Error occurred in the Azure Kinect ({serialNumber}) IMU thread.", device!.SerialNum);
            }
        }
        #endregion

        private void EnsureStop() {
            if (device is null) {
                return;//failed to start;
            }

            if (stopRequested) {
                return;
            }
            stopRequested = true;

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

        #region Azure Kinect Image Unsafe Methods

        private unsafe delegate void* GetUnsafeBuffer();

        private const string GetUnsafeBufferMethodName = nameof(GetUnsafeBuffer);

        private static readonly Lazy<MethodInfo> GetUnsafeBufferMethod = new(() => {
            var method = typeof(KinectImage).GetMethod(GetUnsafeBufferMethodName, BindingFlags.Instance | BindingFlags.NonPublic);
            return method;
        });

        private static unsafe IntPtr GetAzureKinectImageBufferPtr(KinectImage image) {
            /** NOTE:
             *  The Microsoft.Azure.Kinect.Sensor.Image.Memory property will allocate and copy internally, we don't want this copy, so we use its internal method to get the pointer.
             */
            var GetUnsafeBufferDelegate = (GetUnsafeBuffer)GetUnsafeBufferMethod.Value.CreateDelegate(typeof(GetUnsafeBuffer), image);
            var ptr = (IntPtr)GetUnsafeBufferDelegate();
            return ptr;
        }
        #endregion

        #region Helpers
        private void LogAzureKinectExceptionDetail(AzureKinectException ex) {
            Logger?.LogDebug(ex, "AzureKinectException detail:" + ConvertAzureKinectExceptionMessage(ex));
        }

        private static string ConvertAzureKinectExceptionMessage(AzureKinectException ex) {
            var sb = new StringBuilder(ex.Message);
            foreach (var log in ex.LogMessages) {
                sb.Append('\t');
                sb.AppendLine(log.FormatedMessage);
            }
            return sb.ToString();
        }
        #endregion

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
