using System;
using System.Composition;
using OpenSense.Components.Contract;

namespace OpenSense.Components.Imaging {
    [Export(typeof(IComponentMetadata))]
    public class FlipColorVideoMetadata : ConventionalComponentMetadata {

        public override string Description => "Flip color images vertically or horizontally or both.";

        protected override Type ComponentType => typeof(FlipColorVideo);

        public override string Name => "Flip Image";

        protected override string GetPortDescription(string portName) {
            switch (portName) {
                case nameof(FlipColorVideo.In):
                    return "[Required] Images need to be processed.";
                case nameof(FlipColorVideo.Out):
                    return "Processed images.";
                default:
                    return null;
            }
        }

        public override ComponentConfiguration CreateConfiguration() => new FlipColorVideoConfiguration();
    }
}
