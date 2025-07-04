using System;
using Microsoft.Azure.Kinect.BodyTracking;
using Microsoft.Extensions.Logging;
using Microsoft.Psi;

namespace OpenSense.Components.AzureKinect.BodyTracking {
    [Serializable]
    public class AzureKinectBodyTrackerConfiguration : ConventionalComponentConfiguration {
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
        #endregion

        #region ConventionalComponentConfiguration
        public override IComponentMetadata GetMetadata() => new AzureKinectBodyTrackerMetadata();

        protected override object Instantiate(Pipeline pipeline, IServiceProvider? serviceProvider) => new AzureKinectBodyTracker(pipeline) {
            SensorOrientation = SensorOrientation,
            ProcessingBackend = ProcessingBackend,
            GpuDeviceId = GpuDeviceId,
            UseLiteModel = UseLiteModel,
            TemporalSmoothing = TemporalSmoothing,
            Timeout = Timeout,
            ThrowOnTimeout = ThrowOnTimeout,
            OutputNull = OutputNull,
            Logger = serviceProvider?.GetService(typeof(ILogger)) as ILogger,
        };
        #endregion
    }
}
