using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.Reflection;
using OpenSense.Component.Contract;

namespace OpenSense.Pipeline {
    public class ComponentManager: IDisposable {

        private CompositionContainer container;

        [ImportMany(typeof(IComponentMetadata))]
        private IComponentMetadata[] components;

        public IReadOnlyList<IComponentMetadata> Components => components;

        private ComponentManager() {
            var catalog = new AggregateCatalog();
            catalog.Catalogs.Add(new AssemblyCatalog(typeof(ComponentManager).Assembly));
            catalog.Catalogs.Add(new AssemblyCatalog(Assembly.GetEntryAssembly()));
            catalog.Catalogs.Add(new DirectoryCatalog(AppDomain.CurrentDomain.BaseDirectory));
            container = new CompositionContainer(catalog);
            container.ComposeParts(this);
        }

        public void Dispose() {
            container?.Dispose();
            container = null;
        }

        private static Lazy<ComponentManager> instance = new Lazy<ComponentManager>(() => new ComponentManager());

        public static ComponentManager Instance => instance.Value;
    }
}
