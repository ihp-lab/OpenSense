#nullable enable

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.Psi;

namespace OpenSense.Components.Builtin {
    /// <remarks>
    /// For components that have one generic type receiver.
    /// </remarks>
    [Serializable]
    public abstract class GenericComponentConfiguration_OneParam : ComponentConfiguration {

        private readonly MethodInfo _instantiateMethod;

        public GenericComponentConfiguration_OneParam() {
            var method = GetType().GetMethod(nameof(Instantiate), BindingFlags.Instance | BindingFlags.NonPublic);
            _instantiateMethod = method;
        }

        public override sealed object Instantiate(Pipeline pipeline, IReadOnlyList<ComponentEnvironment> instantiatedComponents, IServiceProvider? serviceProvider) {
            /* Create instance */
            var genericInputPortMetadata = GetMetadata().Ports.OfType<GenericComponentPortMetadata_OneParam>().Single(m => m.IsGenericInput);
            var producerMappings = this.GetRemoteProducerMappings(instantiatedComponents);
            var targetMappings = producerMappings
                .Where(m => Equals(m.InputConfiguration.LocalPort.Identifier, genericInputPortMetadata.Identifier))//Find the receiver that is generic
                .ToArray();
            if (targetMappings.Length == 0) {
                throw new MissingRequiredInputConnectionException(Name, genericInputPortMetadata.Name);
            }
            var paramType = targetMappings.Single().RemoteDataType;
            var method = _instantiateMethod.MakeGenericMethod(new[] { paramType });
            var instance = method.Invoke(this, new object?[] { pipeline, serviceProvider, });

            /* Connect the ports */
            foreach (var producerMapping in producerMappings) {
                var inputPortMetadata = (GenericComponentPortMetadata_OneParam)this.FindPortMetadata(producerMapping.InputConfiguration.LocalPort);
                var propertyInfo = inputPortMetadata.GetConstructedPropertyInfo(paramType);
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
        protected abstract object Instantiate<T>(Pipeline pipeline, IServiceProvider? serviceProvider);
    }
}
