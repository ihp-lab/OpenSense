using System;
using System.Collections.Generic;
using System.Composition;
using System.Composition.Hosting;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows;
using OpenSense.Component.Contract;
using OpenSense.Wpf.Component.Contract;

namespace OpenSense.Wpf.Pipeline {
    public class ConfigurationControlCreatorManager{

        [ImportMany]
        private IConfigurationControlCreator[] creators { get; set; }

        public ConfigurationControlCreatorManager() {
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

        public UIElement Create(ComponentConfiguration configuration) {
            var creator = creators.FirstOrDefault(c => c.CanCreate(configuration));
            return creator?.Create(configuration);
        }
    }
}
