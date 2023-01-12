using System;
using Microsoft.Psi;
using OpenSense.Components.Contract;

namespace OpenSense.Components.Psi.Imaging {
    [Serializable]
    public class FlipImageOperatorConfiguration : ConventionalComponentConfiguration {

        private bool horizontal = false;

        public bool Horizontal {
            get => horizontal;
            set => SetProperty(ref horizontal, value);
        }

        private bool vertical = false;

        public bool Vertical {
            get => vertical;
            set => SetProperty(ref vertical, value);
        }

        public override IComponentMetadata GetMetadata() => new FlipImageOperatorMetadata();

        protected override object Instantiate(Pipeline pipeline, IServiceProvider serviceProvider) => new FlipImageOperator(pipeline) { 
            Horizontal = Horizontal, 
            Vertical = Vertical
        };
    }
}
