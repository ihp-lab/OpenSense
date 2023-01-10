using System;
using Microsoft.Extensions.Logging;
using Microsoft.Psi;
using OpenSense.Components.Contract;


namespace OpenSense.Components.OpenFace {
    [Serializable]
    public class OpenFaceConfiguration : ConventionalComponentConfiguration {

        private bool mute = false;

        public bool Mute {
            get => mute;
            set => SetProperty(ref mute, value);
        }

        private float cameraCalibFx = 500;

        public float CameraCalibFx {
            get => cameraCalibFx;
            set => SetProperty(ref cameraCalibFx, value);
        }

        private float cameraCalibFy = 500;

        public float CameraCalibFy {
            get => cameraCalibFy;
            set => SetProperty(ref cameraCalibFy, value);
        }

        private float cameraCalibCx = 640 / 2f;

        public float CameraCalibCx {
            get => cameraCalibCx;
            set => SetProperty(ref cameraCalibCx, value);
        }

        private float cameraCalibCy = 480 / 2f;

        public float CameraCalibCy {
            get => cameraCalibCy;
            set => SetProperty(ref cameraCalibCy, value);
        }

        private bool autoAdjustCenter = false;

        public bool AutoAdjustCenter {
            get => autoAdjustCenter;
            set => SetProperty(ref autoAdjustCenter, value);
        }

        public override IComponentMetadata GetMetadata() => new OpenFaceMetadata();

        protected override object Instantiate(Pipeline pipeline, IServiceProvider serviceProvider) => new OpenFace(pipeline) {
            Logger = (serviceProvider?.GetService(typeof(ILoggerFactory)) as ILoggerFactory)?.CreateLogger(Name),
            Mute = Mute,
            CameraCalibFx = CameraCalibFx,
            CameraCalibFy = CameraCalibFy,
            CameraCalibCx = CameraCalibCx,
            CameraCalibCy = CameraCalibCy,
            AutoAdjustCenter = AutoAdjustCenter,
        };
    }
}
