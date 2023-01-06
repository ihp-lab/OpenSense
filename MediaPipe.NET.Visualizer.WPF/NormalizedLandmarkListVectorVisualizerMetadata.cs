using System;
using System.Composition;
using OpenSense.Components.Contract;

namespace OpenSense.WPF.Components.MediaPipe.NET.Visualizer {
    [Export(typeof(IComponentMetadata))]
    public sealed class NormalizedLandmarkListVectorVisualizerMetadata : ConventionalComponentMetadata  {
        public override string Description => "Visualizes MediaPipe Normalized Landmark List Vector outputs. Requires MediaPipe outputs.";

        protected override Type ComponentType => typeof(NormalizedLandmarkListVectorVisualizer);

        public override string Name => "MediaPipe Landmark Visualizer";

        protected override string GetPortDescription(string portName) {
            switch (portName) {
                case nameof(NormalizedLandmarkListVectorVisualizer.DataIn):
                    return "[Required] MediaPipe outputs.";
                case nameof(NormalizedLandmarkListVectorVisualizer.ImageIn):
                    return "[Required] Images that were send to MediaPipe.";
                case nameof(NormalizedLandmarkListVectorVisualizer.Out):
                    return "Rendered images.";
                default:
                    return null;
            }
        }

        public override ComponentConfiguration CreateConfiguration() => new NormalizedLandmarkListVectorVisualizerConfiguration();
    }
}
