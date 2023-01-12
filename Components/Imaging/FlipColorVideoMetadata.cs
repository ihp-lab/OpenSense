using System;
using System.Composition;
using OpenSense.Components.Contract;

namespace OpenSense.Components.Imaging {
    [Export(typeof(IComponentMetadata))]
    public class FlipImageMetadata : ConventionalComponentMetadata {

        public override string Description => "Flip images vertically or horizontally or both.";

        protected override Type ComponentType => typeof(FlipImage);

        public override string Name => "Flip Image";

        protected override string GetPortDescription(string portName) {
            switch (portName) {
                case nameof(FlipImage.In):
                    return "[Required] Images need to be processed.";
                case nameof(FlipImage.Out):
                    return "Processed images.";
                default:
                    return null;
            }
        }

        public override ComponentConfiguration CreateConfiguration() => new FlipImageConfiguration();
    }
}
