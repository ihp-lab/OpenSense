using System;
using System.Composition;

namespace OpenSense.Components.OpenSmile.Visualizer;

[Export(typeof(IComponentMetadata))]
public sealed class OpenSmileVisualizerMetadata : ConventionalComponentMetadata {

    public override string Name => "openSMILE Visualizer";

    public override string Description => "Plots features of one openSMILE sink over time.";

    protected override Type ComponentType => typeof(OpenSmileVisualizer);

    protected override string? GetPortDescription(string portName) {
        return portName switch {
            nameof(OpenSmileVisualizer.In) => "[Required] Feature frames of one sink of the openSMILE component.",
            nameof(OpenSmileVisualizer.FeatureNamesIn) => "Feature names of the same sink. Without them, features are listed by index.",
            _ => null,
        };
    }

    public override ComponentConfiguration CreateConfiguration() => new OpenSmileVisualizerConfiguration();
}
