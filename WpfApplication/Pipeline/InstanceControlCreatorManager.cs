using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.Linq;
using System.Reflection;
using System.Windows;
using OpenSense.Wpf.Component.Contract;

namespace OpenSense.Wpf.Pipeline {
    public class InstanceControlCreatorManager : IDisposable {

        private CompositionContainer container;

        [ImportMany(typeof(IInstanceControlCreator))]
        private IInstanceControlCreator[] creators;

        private InstanceControlCreatorManager() {
            var catalog = new AggregateCatalog();
            catalog.Catalogs.Add(new AssemblyCatalog(typeof(InstanceControlCreatorManager).Assembly));
            catalog.Catalogs.Add(new AssemblyCatalog(Assembly.GetEntryAssembly()));
            catalog.Catalogs.Add(new DirectoryCatalog(AppDomain.CurrentDomain.BaseDirectory));
            container = new CompositionContainer(catalog);
            try {
                container.ComposeParts(this);
            } catch (CompositionException compositionException) {
                Console.WriteLine(compositionException.ToString());
            }
        }

        public UIElement Create(object instance) {
            var creator = creators.FirstOrDefault(c => c.CanCreate(instance));
            return creator?.Create(instance);
        }

        public void Dispose() {
            container?.Dispose();
            container = null;
        }

        private static Lazy<InstanceControlCreatorManager> instance = new Lazy<InstanceControlCreatorManager>(() => new InstanceControlCreatorManager());

        public static InstanceControlCreatorManager Instance => instance.Value;
    }
}
