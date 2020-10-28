using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using PsiPipeline = Microsoft.Psi.Pipeline;

namespace OpenSense.Component.Contract {
    [Serializable]
    public abstract class ConventionalComponentConfiguration : ComponentConfiguration {

        public sealed override object Instantiate(PsiPipeline pipeline, IReadOnlyList<ComponentEnvironment> instantiatedComponents) {
            var instance = Instantiate(pipeline);
            Debug.Assert(GetMetadata().InputPorts().All(p => p is StaticPortMetadata));
            this.ConnectAllStaticInputs(instance, instantiatedComponents);
            return instance;
        }

        protected abstract object Instantiate(PsiPipeline pipeline);
    }
}
