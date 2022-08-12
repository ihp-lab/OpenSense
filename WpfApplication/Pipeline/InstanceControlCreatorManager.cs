using System;
using System.Collections.Generic;
using System.Composition;
using System.Composition.Hosting;
using System.Linq;
using System.Reflection;
using System.Windows;
using OpenSense.Component.Contract;
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
            assemblies.AddRange(HelperExtensions.LoadAssemblies(AppDomain.CurrentDomain.BaseDirectory));
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
