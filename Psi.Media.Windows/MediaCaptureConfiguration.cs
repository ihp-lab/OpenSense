using System;
using Microsoft.Psi;
using Microsoft.Psi.Media;
using OpenSense.Component.Contract;

namespace OpenSense.Component.Psi.Media {
    [Serializable]
    public class MediaCaptureConfiguration : ConventionalComponentConfiguration {

        private Microsoft.Psi.Media.MediaCaptureConfiguration raw = new Microsoft.Psi.Media.MediaCaptureConfiguration();

        public Microsoft.Psi.Media.MediaCaptureConfiguration Raw {
            get => raw;
            set => SetProperty(ref raw, value);
        }

        public override IComponentMetadata GetMetadata() => new MediaCaptureMetadata();

        protected override object Instantiate(Pipeline pipeline) => new MediaCapture(pipeline, Raw);
    }
}
