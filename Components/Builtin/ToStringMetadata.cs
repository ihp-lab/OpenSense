#nullable enable

using System;
using System.Composition;

namespace OpenSense.Components.Builtin {
    [Export(typeof(IComponentMetadata))]
    public sealed class ToStringMetadata : ConventionalComponentMetadata {

        public override string Name => "ToString";

        public override string Description => "Convert object to string. ToString() method will be called, formatting is not supported at now.";

        protected override Type ComponentType => typeof(ToString);

        public override ComponentConfiguration CreateConfiguration() => new ToStringConfiguration();

        protected override string? GetPortDescription(string portName) {
            switch (portName) {
                case nameof(Builtin.ToString.In):
                    return "[Required] Data to be converted.";
                case nameof(Builtin.ToString.Out):
                    return "Converted string representation, or null.";
                default:
                    return null;
            }
        }
    }
}
