using System;
using System.Collections.Generic;
using System.Composition;
using System.Composition.Hosting;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows;
using OpenSense.Wpf.Component.Contract;

namespace OpenSense.Wpf.Pipeline {
    public class InstanceControlCreatorManager{

        [ImportMany]
        private IInstanceControlCreator[] creators { get; set; }

        public InstanceControlCreatorManager() {
            var assemblies = new List<Assembly>() {
                typeof(ConfigurationControlCreatorManager).Assembly,
                Assembly.GetEntryAssembly(),
            };
            var files = Directory.EnumerateFiles(AppDomain.CurrentDomain.BaseDirectory, "*.dll");
            foreach (var file in files) {
                try {
                    var asm = Assembly.LoadFrom(file);
                    assemblies.Add(asm);
                } catch (BadImageFormatException) {
                }
            }
            var configuration = new ContainerConfiguration()
                .WithAssemblies(assemblies);//note: Fluent interface
            using var container = configuration.CreateContainer();
            container.SatisfyImports(this);
        }

        public UIElement Create(object instance) {
            var creator = creators.FirstOrDefault(c => c.CanCreate(instance));
            return creator?.Create(instance);
        }
    }
}
