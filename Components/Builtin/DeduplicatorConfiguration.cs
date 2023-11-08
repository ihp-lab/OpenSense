#nullable enable

using System;
using Microsoft.Psi;

namespace OpenSense.Components.Builtin {
    [Serializable]
    public sealed class DeduplicatorConfiguration : GenericComponentConfiguration_OneParam {

        private static readonly DeduplicatorMetadata Metadata = new DeduplicatorMetadata();

        private double dullnessInSeconds = 0;

        public double DullnessInSeconds {
            get => dullnessInSeconds;
            set => SetProperty(ref dullnessInSeconds, value);
        }

        private double expirationInSeconds = double.PositiveInfinity;

        public double ExpirationInSeconds {
            get => expirationInSeconds;
            set => SetProperty(ref expirationInSeconds, value);
        }

        public override IComponentMetadata GetMetadata() => Metadata;

        protected override object Instantiate<T>(Pipeline pipeline, IServiceProvider? serviceProvider) => new Deduplicator<T>(pipeline) {
            Dullness = dullnessInSeconds == double.PositiveInfinity ?
                TimeSpan.MaxValue : TimeSpan.FromSeconds(dullnessInSeconds),
            Expiration = expirationInSeconds == double.PositiveInfinity ? 
                TimeSpan.MaxValue : TimeSpan.FromSeconds(expirationInSeconds),
        };
    }
}
