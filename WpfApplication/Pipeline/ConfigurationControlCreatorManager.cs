using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.Linq;
using System.Reflection;
using System.Windows;
using OpenSense.Component.Contract;
using OpenSense.Wpf.Component.Contract;

namespace OpenSense.Wpf.Pipeline {
    public class ConfigurationControlCreatorManager : IDisposable {

        private CompositionContainer container;

        [ImportMany(typeof(IConfigurationControlCreator))]
        private IConfigurationControlCreator[] creators;

        private ConfigurationControlCreatorManager() {
            var catalog = new AggregateCatalog();
            catalog.Catalogs.Add(new AssemblyCatalog(typeof(ConfigurationControlCreatorManager).Assembly));
            catalog.Catalogs.Add(new AssemblyCatalog(Assembly.GetEntryAssembly()));
            catalog.Catalogs.Add(new DirectoryCatalog(AppDomain.CurrentDomain.BaseDirectory));
            container = new CompositionContainer(catalog);
            container.ComposeParts(this);
        }

        public UIElement Create(ComponentConfiguration configuration) {
            var creator = creators.FirstOrDefault(c => c.CanCreate(configuration));
            return creator?.Create(configuration);
        }

        public void Dispose() {
            container?.Dispose();
            container = null;
        }

        private static Lazy<ConfigurationControlCreatorManager> instance = new Lazy<ConfigurationControlCreatorManager>(() => new ConfigurationControlCreatorManager());

        public static ConfigurationControlCreatorManager Instance => instance.Value;
    }
}
