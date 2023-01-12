using System;
using Microsoft.Psi;
using Microsoft.Psi.Imaging;
using OpenSense.Components.Contract;

namespace OpenSense.Components.Psi.Imaging {
    [Serializable]
    public sealed class ResizeImageOperatorConfiguration : ConventionalComponentConfiguration {

        private int width = 640;

        public int Width {
            get => width;
            set => SetProperty(ref width, value);
        }

        private int height = 480;

        public int Height {
            get => height;
            set => SetProperty(ref height, value);
        }

        private SamplingMode samplingMode = SamplingMode.Bilinear;

        public SamplingMode SamplingMode {
            get => samplingMode;
            set => SetProperty(ref samplingMode, value);
        }

        private bool bypassIfPossible = true;

        public bool BypassIfPossible {
            get => bypassIfPossible;
            set => SetProperty(ref bypassIfPossible, value);
        }

        public override IComponentMetadata GetMetadata() => new ResizeImageOperatorMetadata();

        protected override object Instantiate(Pipeline pipeline, IServiceProvider serviceProvider) => new ResizeImageOperator(pipeline) {
            Width = Width,
            Height = Height,
            SamplingMode = SamplingMode,
            BypassIfPossible = BypassIfPossible,
        };
    }
}
