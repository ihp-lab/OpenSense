using System;
using System.Collections.Generic;
using System.Composition;
using System.Composition.Hosting;
using System.Reflection;
using OpenSense.Components.Contract;

namespace OpenSense.Pipeline {
    public class ComponentManager {

        [ImportMany]
        private IComponentMetadata[] components { get; set; }

        public IReadOnlyList<IComponentMetadata> Components => components;

        public ComponentManager() {
            var assemblies = new List<Assembly>() {
                typeof(ComponentManager).Assembly,
                Assembly.GetEntryAssembly(),
            };
            assemblies.AddRange(HelperExtensions.LoadAssemblies(AppDomain.CurrentDomain.BaseDirectory));
            var configuration = new ContainerConfiguration()
                .WithAssemblies(assemblies);//note: Fluent interface
            using var container = configuration.CreateContainer();
            container.SatisfyImports(this);
        }
    }
}
