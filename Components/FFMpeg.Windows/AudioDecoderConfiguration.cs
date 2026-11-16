using System;
using Microsoft.Extensions.Logging;
using Microsoft.Psi;

namespace OpenSense.Components.FFMpeg;

[Serializable]
public sealed class AudioDecoderConfiguration : ConventionalComponentConfiguration {

    #region Options
    private bool discardRemainingOnStop;

    public bool DiscardRemainingOnStop {
        get => discardRemainingOnStop;
        set => SetProperty(ref discardRemainingOnStop, value);
    }
    #endregion

    public override IComponentMetadata GetMetadata() => new AudioDecoderMetadata();

    protected override object Instantiate(Pipeline pipeline, IServiceProvider serviceProvider) => new AudioDecoder(pipeline) {
        DiscardRemainingOnStop = DiscardRemainingOnStop,
        Logger = (serviceProvider?.GetService(typeof(ILoggerFactory)) as ILoggerFactory)?.CreateLogger(Name),
    };
}
