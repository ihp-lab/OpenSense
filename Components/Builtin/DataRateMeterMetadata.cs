#nullable enable

using System;
using System.Composition;
using OpenSense.Components.Contract;

namespace OpenSense.Components.Builtin {
    [Export(typeof(IComponentMetadata))]
    public class DataRateMeterMetadata : ConventionalComponentMetadata {
        public override string Description => "Measures data rate.";

        protected override Type ComponentType => typeof(DataRateMeter);

        public override string Name => "Data Rate Meter";
        protected override string? GetPortDescription(string portName) {
            switch (portName) {
                case nameof(DataRateMeter.In):
                    return "[Required] Data stream to be measured.";
                case nameof(DataRateMeter.Out):
                    return "Data rate.";
                default:
                    return null;
            }
        }

        public override ComponentConfiguration CreateConfiguration() => new DataRateMeterConfiguration();
    }
}
