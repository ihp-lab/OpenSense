using System;
using System.Composition;

namespace OpenSense.Components.OpenSmile;

[Export(typeof(IComponentMetadata))]
public sealed class OpenSmileMetadata : ConventionalComponentMetadata {

    public override string Name => "openSMILE";

    public override string Description => "openSMILE by audEERING GmbH for audio feature extraction. Requires openSMILE pipeline configuration file.";

    protected override Type ComponentType => typeof(OpenSmile);

    protected override string? GetPortDescription(string portName) {
        return portName switch {
            nameof(OpenSmile.In) => "[Required] Named audio signals. Name should match a cExternalAudioSource in the openSMILE configuration file. Audio format should match the configuration file too.",
            nameof(OpenSmile.Out) => "Named features. Name should match a cExternalSink in the openSMILE configuration file.",
            nameof(OpenSmile.FeatureNamesOut) => "Named lists of feature names, in the same order as the values in Out. Name should match a cExternalSink in the openSMILE configuration file. Sent once when the pipeline starts.",
            _ => null,
        };
    }

    public override ComponentConfiguration CreateConfiguration() => new OpenSmileConfiguration();
}
