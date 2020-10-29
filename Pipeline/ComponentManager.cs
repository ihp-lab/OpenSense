using System;
using System.Collections.Generic;
using System.Composition;
using System.Composition.Hosting;
using System.IO;
using System.Reflection;
using OpenSense.Component.Contract;

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
            var files = Directory.EnumerateFiles(AppDomain.CurrentDomain.BaseDirectory, "*.dll");
            foreach (var file in files) {
                var asm = Assembly.LoadFrom(file);
                assemblies.Add(asm);
            }
            var configuration = new ContainerConfiguration()
                .WithAssemblies(assemblies);//note: Fluent interface
            using var container = configuration.CreateContainer();
            container.SatisfyImports(this);
        }
    }
}
