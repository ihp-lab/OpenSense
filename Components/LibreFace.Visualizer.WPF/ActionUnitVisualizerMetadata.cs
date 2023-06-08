using System;
using System.Composition;
using OpenSense.Components.Contract;

namespace OpenSense.Components.LibreFace.Visualizer {
    [Export(typeof(IComponentMetadata))]
    public class ActionUnitVisualizerMetadata : ConventionalComponentMetadata {

        public override string Description => "Visualize Action Unit values.";

        protected override Type ComponentType => typeof(ActionUnitVisualizer);

        public override string Name => "Action Unit Visualizer";

        protected override string GetPortDescription(string portName) {
            switch (portName) {
                case nameof(ActionUnitVisualizer.In):
                    return "[Required] Action Unit values to be visualized.";
                case nameof(ActionUnitVisualizer.Out):
                    return "Converted texts.";
                default:
                    return null;
            }
        }

        public override ComponentConfiguration CreateConfiguration() => new ActionUnitVisualizerConfiguration();
    }
}
