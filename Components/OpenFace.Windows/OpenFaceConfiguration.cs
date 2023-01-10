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

        private float focalLengthX = 500;

        public float FocalLengthX {
            get => focalLengthX;
            set => SetProperty(ref focalLengthX, value);
        }

        private float focalLengthY = 500;

        public float FocalLengthY {
            get => focalLengthY;
            set => SetProperty(ref focalLengthY, value);
        }

        private float centerX = 640 / 2f;

        public float CenterX {
            get => centerX;
            set => SetProperty(ref centerX, value);
        }

        private float centerY = 480 / 2f;

        public float CenterY {
            get => centerY;
            set => SetProperty(ref centerY, value);
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
            FocalLengthX = FocalLengthX,
            FocalLengthY = FocalLengthY,
            CenterX = CenterX,
            CenterY = CenterY,
            AutoAdjustCenter = AutoAdjustCenter,
        };
    }
}
