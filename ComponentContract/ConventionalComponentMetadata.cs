using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Microsoft.Psi;

namespace OpenSense.Components.Contract {
    public abstract class ConventionalComponentMetadata : IComponentMetadata {

        protected abstract Type ComponentType { get; }

        protected virtual PropertyInfo[] IgnoreProperties => Array.Empty<PropertyInfo>();

        public virtual string Name => ComponentType.FullName;

        public virtual string Description => "";

        protected virtual string GetPortDescription(string portName) => null;

        public IReadOnlyList<IPortMetadata> Ports {
            get {
                var resultProps = new List<PropertyInfo>();
                var props = ComponentType
                    .GetProperties(BindingFlags.Public | BindingFlags.Instance)
                    .Where(p => p.CanRead)
                    .Except(IgnoreProperties)
                    .ToArray();

                try {
                    //regular
                    var basicTypes = new Type[] { typeof(IConsumer<>), typeof(IProducer<>) };
                    var regular = props
                        .Where(p =>
                            basicTypes.Any(t => p.PropertyType.IsAssignableToGenericType(t))
                        );
                    resultProps.AddRange(regular);
                    //list
                    var list = props
                        .Where(p =>
                            p.PropertyType.IsAssignableToGenericType(typeof(IReadOnlyList<>))
                                && basicTypes.Any(t => p.PropertyType.GetGenericArguments()[0].IsAssignableToGenericType(t))
                        );
                    resultProps.AddRange(list);
                    //dict
                    var dict = props
                        .Where(p =>
                            p.PropertyType.IsAssignableToGenericType(typeof(IReadOnlyDictionary<,>))
                                && p.PropertyType.GetGenericArguments()[0] == typeof(string)
                                && basicTypes.Any(t => p.PropertyType.GetGenericArguments()[1].IsAssignableToGenericType(t))
                        );
                    resultProps.AddRange(dict);
                } catch (FileNotFoundException ex) {// missing dependent dll
                    throw new Exception("Missing dependent DLLs", ex);
                }

                var result = resultProps
                    .Select(p => {
                        var description = GetPortDescription(p.Name);
                        var result = new StaticPortMetadata(p, description: description);
                        return result;
                    })
                    .ToArray();
                return result;
            }
        }

        public abstract ComponentConfiguration CreateConfiguration();

        public IProducer<T> GetProducer<T>(object instance, PortConfiguration portConfiguration) => this.GetStaticProducer<T>(instance, portConfiguration);
    }
}
