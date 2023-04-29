using System;
using System.Collections.Generic;
using System.Composition;
using System.Diagnostics;
using System.Linq;
using Microsoft.Psi;
using OpenSense.Components.Contract;

namespace OpenSense.Components.CollectionOperators {
    [Export(typeof(IComponentMetadata))]
    public sealed class ElementAtMetadata : IComponentMetadata {

        public string Name => "Element At";

        public string Description => "Return the element at a given index. No element will be returned if the index is out of range.";

        public IReadOnlyList<IPortMetadata> Ports => FindPorts(typeof(ElementAt<>));

        public ComponentConfiguration CreateConfiguration() => new ElementAtConfiguration();

        public IProducer<T> GetProducer<T>(object instance, PortConfiguration portConfiguration) {
            Debug.Assert(Equals(this.OutputPorts().Single().Identifier, portConfiguration.Identifier));
            Debug.Assert(instance != null && HelperExtensions.CanProducerResultBeCastTo<T>(instance));
            var result = HelperExtensions.CastProducerResult<T>(instance);
            return result;
        }

        #region Helpers
        private static IReadOnlyList<IPortMetadata> FindPorts(Type componentType) {
            var properties = HelperExtensions.FindPortProperties(componentType);
            var result = new List<IPortMetadata>();
            foreach (var property in properties) {
                var aggregation = HelperExtensions.FindPortAggregation(property);
                var direction = HelperExtensions.FindPortDirection(property, aggregation);
                var dataType = HelperExtensions.FindPortDataType(property, aggregation);
                IPortMetadata port;
                if (dataType.IsGenericType && dataType.GenericTypeArguments[0].IsGenericTypeParameter) {
                    Debug.Assert(direction == PortDirection.Input);
                    Debug.Assert(property.Name == nameof(ElementAt<int>.In));
                    port = new ElementAtPortMetadata(property, direction, description: "[Required] The collection.");
                } else if (dataType.IsGenericParameter) {
                    Debug.Assert(direction == PortDirection.Output);
                    Debug.Assert(property.Name == nameof(ElementAt<int>.Out));
                    port = new ElementAtPortMetadata(property, direction, description: "The element.");
                } else {
                    Debug.Assert(direction == PortDirection.Input);
                    Debug.Assert(property.Name == nameof(ElementAt<int>.IndexIn));
                    port = new StaticPortMetadata(property, description: "[Optional] The index.");
                }
                result.Add(port);
            }
            return result;
        }
        #endregion
    }
}
