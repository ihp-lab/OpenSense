using System;
using System.Composition;
using OpenSense.Components.Contract;

namespace OpenSense.Components.LibreFace.Visualizer {
    [Export(typeof(IComponentMetadata))]
    public class ActionUnitIntensityVisualizerMetadata : ConventionalComponentMetadata {

        public override string Description => "Visualize Action Unit intensity values.";

        protected override Type ComponentType => typeof(ActionUnitIntensityVisualizer);

        public override string Name => "Action Unit Intensity Visualizer";

        protected override string GetPortDescription(string portName) {
            switch (portName) {
                case nameof(ActionUnitIntensityVisualizer.In):
                    return "[Required] Action Unit intensity values to be visualized.";
                default:
                    return null;
            }
        }

        public override ComponentConfiguration CreateConfiguration() => new ActionUnitIntensityVisualizerConfiguration();
    }
}
