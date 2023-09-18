using System;
using System.Composition;

namespace OpenSense.Components.LibreFace.Visualizer {
    [Export(typeof(IComponentMetadata))]
    public class ActionUnitPresenceVisualizerMetadata : ConventionalComponentMetadata {

        public override string Description => "Visualize Action Unit presence values.";

        protected override Type ComponentType => typeof(ActionUnitPresenceVisualizer);

        public override string Name => "Action Unit Presence Visualizer";

        protected override string GetPortDescription(string portName) {
            switch (portName) {
                case nameof(ActionUnitPresenceVisualizer.In):
                    return "[Required] Action Unit presence values to be visualized.";
                default:
                    return null;
            }
        }

        public override ComponentConfiguration CreateConfiguration() => new ActionUnitPresenceVisualizerConfiguration();
    }
}
