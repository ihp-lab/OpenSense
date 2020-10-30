using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Microsoft.Psi;

namespace OpenSense.Component.Contract {
    public abstract class ConventionalComponentMetadata : IComponentMetadata {

        protected abstract Type ComponentType { get; }

        public virtual string Name => ComponentType.FullName;

        public virtual string Description => "";

        public IReadOnlyList<IPortMetadata> Ports => ComponentType
                    .GetProperties(BindingFlags.Public | BindingFlags.Instance)
                    .Where(p => p.CanRead)
                    .Where(p => new Type[] { typeof(IConsumer<>), typeof(IProducer<>) }.Any(t => p.PropertyType.IsAssignableToGenericType(t)))
                    .Select(p => new StaticPortMetadata(p))
                    .ToArray();

        public abstract ComponentConfiguration CreateConfiguration();

        public object GetConnector<T>(object instance, PortConfiguration portConfiguration) => this.GetStaticProducer<T>(instance, portConfiguration);
    }
}
