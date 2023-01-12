using System;
using Microsoft.Psi;
using OpenSense.Components.Contract;

namespace OpenSense.Components.Imaging {
    [Serializable]
    public class FlipImageConfiguration : ConventionalComponentConfiguration {

        private bool flipHorizontal = false;

        public bool FlipHorizontal {
            get => flipHorizontal;
            set => SetProperty(ref flipHorizontal, value);
        }

        private bool flipVertical = false;

        public bool FlipVertical {
            get => flipVertical;
            set => SetProperty(ref flipVertical, value);
        }

        public override IComponentMetadata GetMetadata() => new FlipImageMetadata();

        protected override object Instantiate(Pipeline pipeline, IServiceProvider serviceProvider) => new FlipImage(pipeline) { FlipHorizontal = FlipHorizontal, FlipVertical = FlipVertical };
    }
}
