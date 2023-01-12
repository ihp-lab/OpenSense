using System;
using System.Composition;
using OpenSense.Components.Contract;

namespace OpenSense.Components.Psi.Imaging {
    [Export(typeof(IComponentMetadata))]
    public class FlipImageOperatorMetadata : ConventionalComponentMetadata {

        public override string Description => "Flip images vertically or horizontally or both.";

        protected override Type ComponentType => typeof(FlipImageOperator);

        public override string Name => "Flip Image";

        protected override string GetPortDescription(string portName) {
            switch (portName) {
                case nameof(FlipImageOperator.In):
                    return "[Required] Images need to be processed.";
                case nameof(FlipImageOperator.Out):
                    return "Processed images.";
                default:
                    return null;
            }
        }

        public override ComponentConfiguration CreateConfiguration() => new FlipImageOperatorConfiguration();
    }
}
