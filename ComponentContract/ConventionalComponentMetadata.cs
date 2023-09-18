using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.Psi;

namespace OpenSense.Components {
    public abstract class ConventionalComponentMetadata : IComponentMetadata {

        protected abstract Type ComponentType { get; }

        protected virtual PropertyInfo[] IgnoreProperties => Array.Empty<PropertyInfo>();

        protected virtual string GetPortDescription(string portName) => null;

        #region IComponentMetadata
        public virtual string Name => ComponentType.FullName;

        public virtual string Description => "";

        public IReadOnlyList<IPortMetadata> Ports =>
            FindPorts(ComponentType, IgnoreProperties, GetPortDescription);

        public abstract ComponentConfiguration CreateConfiguration();

        public IProducer<T> GetProducer<T>(object instance, PortConfiguration portConfiguration) =>
            this.GetStaticProducer<T>(instance, portConfiguration);

        #endregion

        #region Helpers
        private static IReadOnlyList<IPortMetadata> FindPorts(Type componentType, PropertyInfo[] ignoreProperties, Func<string, string> getPortDescription) {
            var props = HelperExtensions
                .FindPortProperties(componentType)
                .Except(ignoreProperties);

            var result = props
                .Select(p => {
                    var description = getPortDescription(p.Name);
                    var result = new StaticPortMetadata(p, description: description);
                    return result;
                })
                .ToArray();
            return result;
        }
        #endregion
    }
}
