using System;
using System.Composition;

namespace OpenSense.Components.HM {
    [Export(typeof(IComponentMetadata))]
    public sealed class HevcDecoderMetadata : ConventionalComponentMetadata {

        public override string Name => "HM HEVC Decoder";

        public override string Description => "[Experimental] Decode HEVC Access Units into Picture frames using HM decoder. Due to HM limitations, all HM components are serialized at runtime.";

        protected override Type ComponentType => typeof(HevcDecoder);

        protected override string? GetPortDescription(string portName) {
            return portName switch {
                nameof(HevcDecoder.In) => "[Required] HEVC Access Unit stream. Must use a non-dropping DeliveryPolicy.",
                nameof(HevcDecoder.Out) => "Decoded Picture stream containing raw pixel data and metadata (SPS, POC).",
                _ => null,
            };
        }

        public override ComponentConfiguration CreateConfiguration() => new HevcDecoderConfiguration();
    }
}
