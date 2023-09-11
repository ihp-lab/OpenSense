#nullable enable

using System;
using Microsoft.Psi;
using OpenSense.Components.Contract;

namespace OpenSense.Components.Builtin {
    [Serializable]
    public class DataRateMeterConfiguration : ConventionalComponentConfiguration {

        public override IComponentMetadata GetMetadata() => new DataRateMeterMetadata();

        protected override object Instantiate(Pipeline pipeline, IServiceProvider serviceProvider) => new DataRateMeter(pipeline) {
        };
    }
}
