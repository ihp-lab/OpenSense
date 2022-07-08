using System;
using System.Composition;
using OpenSense.Component.Contract;

namespace OpenSense.Component.OpenFace.Visualizer {
    [Export(typeof(IComponentMetadata))]
    public class OpenFaceVisualizerMetadata : ConventionalComponentMetadata {

        public override string Description => "Visualizes OpenFace outputs. Requires OpenFace outputs.";

        protected override Type ComponentType => typeof(OpenFaceVisualizer);

        public override string Name => "OpenFace Visualizer";

        protected override string GetPortDescription(string portName) {
            switch (portName) {
                case nameof(OpenFaceVisualizer.DataIn):
                    return "[Required] OpenFace outputs.";
                case nameof(OpenFaceVisualizer.ImageIn):
                    return "[Required] Images that were send to OpenFace.";
                case nameof(OpenFaceVisualizer.Out):
                    return "Rendered images.";
                default:
                    return null;
            }
        }

        public override ComponentConfiguration CreateConfiguration() => new OpenFaceVisualizerConfiguration();
    }
}
