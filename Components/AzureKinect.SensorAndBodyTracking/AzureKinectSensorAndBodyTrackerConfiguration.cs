using System;
using System.Threading;
using Microsoft.Azure.Kinect.BodyTracking;
using Microsoft.Azure.Kinect.Sensor;
using Microsoft.Extensions.Logging;
using Microsoft.Psi;
using OpenSense.Components.AzureKinect.Sensor;

namespace OpenSense.Components.AzureKinect.SensorAndBodyTracking {
    [Serializable]
    public sealed class AzureKinectSensorAndBodyTrackerConfiguration : ConventionalComponentConfiguration {
        #region Options
        #region Sensor
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

        private int brightness = -1;

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
        #endregion

        #region Body Tracker
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
        #endregion
        #endregion

        #region ConventionalComponentConfiguration
        public override IComponentMetadata GetMetadata() => new AzureKinectSensorAndBodyTrackerMetadata();

        protected override object Instantiate(Pipeline pipeline, IServiceProvider? serviceProvider) => new AzureKinectSensorAndBodyTracker(pipeline) {
            #region Sensor
            DeviceIndex = DeviceIndex,
            ColorResolution = ColorResolution,
            ColorFormat = ColorFormat,
            FrameRate = FrameRate,
            DepthMode = DepthMode,
            DepthDelayOffColor = DepthDelayOffColor,
            SynchronizedImagesOnly = SynchronizedImagesOnly,
            CaptureThreadPriority = CaptureThreadPriority,
            CaptureSampleTimeout = CaptureSampleTimeout,
            CaptureThreadJoinTimeout = CaptureThreadJoinTimeout,
            EnableIMU = EnableIMU,
            ImuThreadPriority = ImuThreadPriority,
            ImuSampleTimeout = ImuSampleTimeout,
            ImuThreadJoinTimeout = ImuThreadJoinTimeout,
            WiredSyncMode = WiredSyncMode,
            SuboridinateDelayOffMaster = SuboridinateDelayOffMaster,
            ExposureTime = ExposureTime,
            Brightness = Brightness,
            Contrast = Contrast,
            Saturation = Saturation,
            Sharpness = Sharpness,
            WhiteBalance = WhiteBalance,
            BacklightCompensation = BacklightCompensation,
            Gain = Gain,
            PowerlineFrequency = PowerlineFrequency,
            StreamingIndicator = StreamingIndicator,
            #endregion

            #region Body Tracker
            SensorOrientation = SensorOrientation,
            ProcessingBackend = ProcessingBackend,
            GpuDeviceId = GpuDeviceId,
            UseLiteModel = UseLiteModel,
            TemporalSmoothing = TemporalSmoothing,
            Timeout = Timeout,
            ThrowOnTimeout = ThrowOnTimeout,
            OutputNull = OutputNull,
            #endregion

            Logger = serviceProvider?.GetService(typeof(ILogger)) as ILogger,
        };
        #endregion
    }
}
