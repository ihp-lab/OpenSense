using System;
using System.Composition;

namespace OpenSense.Components.HM {
    [Export(typeof(IComponentMetadata))]
    public sealed class HevcEncoderMetadata : ConventionalComponentMetadata {

        public override string Name => "HM HEVC Encoder";

        public override string Description => "[Experimental] Encode Picture frames into HEVC Access Units using HM encoder. Due to HM limitations, all HM components are serialized at runtime.";

        protected override Type ComponentType => typeof(HevcEncoder);

        protected override string? GetPortDescription(string portName) {
            return portName switch {
                nameof(HevcEncoder.In) => "[Required] Input Picture stream. Supports any ChromaFormat and bit depth.",
                nameof(HevcEncoder.Out) => "Encoded HEVC Access Unit stream.",
                _ => null,
            };
        }

        public override ComponentConfiguration CreateConfiguration() => new HevcEncoderConfiguration();
    }
}
