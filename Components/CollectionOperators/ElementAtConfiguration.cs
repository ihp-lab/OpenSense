using System;
using Microsoft.Psi;
using OpenSense.Components.Contract;

namespace OpenSense.Components.CollectionOperators {
    [Serializable]
    public sealed class ElementAtConfiguration : ConventionalComponentConfiguration_OneParam {

        private int index = 0;

        public int Index {
            get => index;
            set => SetProperty(ref index, value);
        }

        public override IComponentMetadata GetMetadata() => new ElementAtMetadata();

        protected override object Instantiate<T>(Pipeline pipeline, IServiceProvider serviceProvider) =>
            new ElementAt<T>(pipeline) { Index = Index, };
    }
}
