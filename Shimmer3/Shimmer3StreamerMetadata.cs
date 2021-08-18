using System;
using System.Composition;
using OpenSense.Component.Contract;

namespace OpenSense.Component.Shimmer3 {
    [Export(typeof(IComponentMetadata))]
    public class Shimmer3StreamerMetadata : ConventionalComponentMetadata {

        public override string Description => "Connect Shimmer 3 via Bluetooth and streaming data.";

        protected override Type ComponentType => typeof(Shimmer3Streamer);

        public override ComponentConfiguration CreateConfiguration() => new Shimmer3StreamerConfiguration();
    }
}
