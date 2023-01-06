using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using PsiPipeline = Microsoft.Psi.Pipeline;

namespace OpenSense.Components.Contract {
    [Serializable]
    public abstract class ConventionalComponentConfiguration : ComponentConfiguration {

        public sealed override object Instantiate(PsiPipeline pipeline, IReadOnlyList<ComponentEnvironment> instantiatedComponents, IServiceProvider serviceProvider) {
            var instance = Instantiate(pipeline, serviceProvider);
            Debug.Assert(GetMetadata().InputPorts().All(p => p is StaticPortMetadata));
            this.ConnectAllStaticInputs(instance, instantiatedComponents);
            return instance;
        }

        /// <summary>
        /// This method is called to initialize an instance. After the instance is returned, connections of ports will be added.
        /// </summary>
        /// <param name="pipeline">The pipeline will be connected to.</param>
        /// <param name="serviceProvider">A <see cref="IServiceProvider"/> can be used as needed. Can be <see cref="null"/>.</param>
        /// <returns>An instance of the component initialized using the current configuration.</returns>
        protected abstract object Instantiate(PsiPipeline pipeline, IServiceProvider serviceProvider);
    }
}
