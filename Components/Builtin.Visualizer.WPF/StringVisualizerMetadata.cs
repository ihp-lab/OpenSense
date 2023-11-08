#nullable enable

using System;
using OpenSense.Components;
using System.Composition;

namespace OpenSense.WPF.Components.Builtin.Visualizer {
    [Export(typeof(IComponentMetadata))]
    public class StringVisualizerMetadata : ConventionalComponentMetadata {

        public override string Description => "Visualize string values.";

        protected override Type ComponentType => typeof(StringVisualizer);

        public override string Name => "String Visualizer";

        protected override string? GetPortDescription(string portName) {
            switch (portName) {
                case nameof(StringVisualizer.In):
                    return "[Required] String values to be visualized.";
                default:
                    return null;
            }
        }

        public override ComponentConfiguration CreateConfiguration() => new StringVisualizerConfiguration();
    }
}
