using System;
using System.Composition;
using OpenSense.Components.Contract;

namespace OpenSense.Components.Psi.Imaging {
    [Export(typeof(IComponentMetadata))]
    public class ResizeImageOperatorMetadata : ConventionalComponentMetadata {

        public override string Description => "Resizes images.";

        protected override Type ComponentType => typeof(ResizeImageOperator);

        public override string Name => "Resize Image";

        protected override string GetPortDescription(string portName) {
            switch (portName) {
                case nameof(ResizeImageOperator.In):
                    return "[Required] Images need to be processed.";
                case nameof(ResizeImageOperator.Out):
                    return "Processed images.";
                default:
                    return null;
            }
        }

        public override ComponentConfiguration CreateConfiguration() => new ResizeImageOperatorConfiguration();
    }
}
