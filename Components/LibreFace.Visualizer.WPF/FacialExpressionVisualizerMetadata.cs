using System;
using System.Composition;

namespace OpenSense.Components.LibreFace.Visualizer {
    [Export(typeof(IComponentMetadata))]
    public class FacialExpressionVisualizerMetadata : ConventionalComponentMetadata {

        public override string Description => "Visualize facial expression values.";

        protected override Type ComponentType => typeof(FacialExpressionVisualizer);

        public override string Name => "Facial Expression Visualizer";

        protected override string GetPortDescription(string portName) {
            switch (portName) {
                case nameof(FacialExpressionVisualizer.In):
                    return "[Required] Facial expression values to be visualized.";
                case nameof(FacialExpressionVisualizer.Out):
                    return "Converted texts.";
                default:
                    return null;
            }
        }

        public override ComponentConfiguration CreateConfiguration() => new FacialExpressionVisualizerConfiguration();
    }
}
