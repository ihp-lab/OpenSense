using System;
using Microsoft.Psi;

namespace OpenSense.Components.OpenSmile.Visualizer;

[Serializable]
public sealed class OpenSmileVisualizerConfiguration : ConventionalComponentConfiguration {

    #region Options
    private double windowInSeconds = 10;

    public double WindowInSeconds {
        get => windowInSeconds;
        set => SetProperty(ref windowInSeconds, value);
    }
    #endregion

    public override IComponentMetadata GetMetadata() => new OpenSmileVisualizerMetadata();

    protected override object Instantiate(Pipeline pipeline, IServiceProvider serviceProvider) => new OpenSmileVisualizer(pipeline) {
        Window = TimeSpan.FromSeconds(WindowInSeconds),
    };
}
