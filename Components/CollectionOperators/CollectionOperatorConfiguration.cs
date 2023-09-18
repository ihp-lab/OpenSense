using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using Microsoft.Psi;
using OpenSense.Components.Builtin;

namespace OpenSense.Components.CollectionOperators {
    /// <remarks>
    /// For components that have one generic collection type receiver.
    /// </remarks>
    [Serializable]
    public abstract class CollectionOperatorConfiguration : ComponentConfiguration {

        private static readonly MethodInfo InstantiateMethod =
            typeof(CollectionOperatorConfiguration)
            .GetMethod(nameof(Instantiate), BindingFlags.Instance | BindingFlags.NonPublic);

        public sealed override object Instantiate(Pipeline pipeline, IReadOnlyList<ComponentEnvironment> instantiatedComponents, IServiceProvider serviceProvider) {
            /* Create instance */
            var genericInputPortMetadata = GetMetadata().Ports
                .Where(m => (m is GenericComponentPortMetadata_OneParam gMetadata && gMetadata.IsGenericInput) || (m is CollectionOperatorPortMetadata cMetadata && cMetadata.IsGenericCollectionInput))
                .Single();
            var producerMappings = this.GetRemoteProducerMappings(instantiatedComponents);
            var targetMappings = producerMappings
                .Where(m => Equals(m.InputConfiguration.LocalPort.Identifier, genericInputPortMetadata.Identifier))//Find the receiver that is generic
                .ToArray();
            if (targetMappings.Length == 0) {
                throw new MissingRequiredInputConnectionException(Name, genericInputPortMetadata.Name);
            }
            var collectionType = targetMappings.Single().RemoteDataType;
            var elemType = HelperExtensions.FindElementTypesOfCollectionType(collectionType).Single();
            Debug.Assert(InstantiateMethod is not null);
            var method = InstantiateMethod.MakeGenericMethod(new[] { elemType, collectionType, });
            var instance = method.Invoke(this, new object[] { pipeline, serviceProvider, });

            /* Connect the ports */
            foreach (var producerMapping in producerMappings) {
                PropertyInfo propertyInfo;
                var inputPortMetadata = this.FindPortMetadata(producerMapping.InputConfiguration.LocalPort);
                switch (inputPortMetadata) {
                    case GenericComponentPortMetadata_OneParam gMetadata:
                        propertyInfo = gMetadata.GetConstructedPropertyInfo(elemType, collectionType);
                        break;
                    case CollectionOperatorPortMetadata cMetadata:
                        propertyInfo = cMetadata.GetConstructedPropertyInfo(elemType, collectionType);
                        break;
                    default:
                        throw new InvalidOperationException();
                }
                dynamic consumer = propertyInfo.GetValue(instance);
                var consumerType = HelperExtensions.GetConsumerResultType(consumer);
                var getProducerFunc = typeof(HelperExtensions)
                    .GetMethod(nameof(HelperExtensions.GetProducer))
                    .MakeGenericMethod(consumerType);//Note: not the producerMapping.Producer here, so that type conversion is applied if needed
                dynamic producer = getProducerFunc.Invoke(null, new object[] { producerMapping.Remote, producerMapping.InputConfiguration.RemotePort });
                Operators.PipeTo(producer, consumer, producerMapping.InputConfiguration.DeliveryPolicy);
            }

            return instance;
        }

        /// <summary>
        /// This method is called to initialize an instance. After the instance is returned, connections of ports will be added.
        /// </summary>
        /// <param name="pipeline">The pipeline will be connected to.</param>
        /// <param name="serviceProvider">A <see cref="IServiceProvider"/> can be used as needed. Can be <see cref="null"/>.</param>
        /// <returns>An instance of the component initialized using the current configuration.</returns>
        protected abstract object Instantiate<TElem, TCollection>(Pipeline pipeline, IServiceProvider serviceProvider)
            where TCollection : IEnumerable<TElem>;
    }
}
