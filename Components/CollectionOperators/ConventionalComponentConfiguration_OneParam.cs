using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using Microsoft.Psi;
using OpenSense.Components.Contract;

namespace OpenSense.Components.CollectionOperators {
    [Serializable]
    public abstract class ConventionalComponentConfiguration_OneParam : ComponentConfiguration {

        private static readonly MethodInfo InstantiateMethod =
            typeof(ConventionalComponentConfiguration_OneParam)
            .GetMethod(nameof(Instantiate), BindingFlags.Instance | BindingFlags.NonPublic);

        public sealed override object Instantiate(Pipeline pipeline, IReadOnlyList<ComponentEnvironment> instantiatedComponents, IServiceProvider serviceProvider) {
            /* Create instance */
            var producers = this.GetRemoteProducers(instantiatedComponents);
            var producerInfo = producers.Select(AnalyzeProducer)
                .ToArray();
            var (targetProducer, producerCollectionType, elementTypes) = producerInfo.Single(i => i.ElementTypes.Count == 1);
            var elementType = elementTypes.Single();
            Debug.Assert(InstantiateMethod is not null);
            var method = InstantiateMethod.MakeGenericMethod(new[] { elementType });
            var instance = method.Invoke(this, new object[] { pipeline, serviceProvider, });

            /* Connect static ports */
            this.ConnectAllStaticInputs(instance, instantiatedComponents);

            /* Connect the generic port (similar to ConnectAllStaticInputs()) */
            var portInfo = Inputs
                .Select(i => (i, this.FindPortMetadata(i.LocalPort)))
                .ToArray();
            var (inputPortConfig, inputPortMetadata) = portInfo
                .Where(t => t.Item2 is ElementAtPortMetadata)
                .Select(t => (t.i, (ElementAtPortMetadata)t.Item2))
                .Single();
            Debug.Assert(inputPortMetadata.Direction == PortDirection.Input);
            var property = inputPortMetadata.GetProperty(elementType);
            dynamic consumer = property.GetValue(instance);
            var consumerCollectionType = HelperExtensions.GetConsumerResultType(consumer);
            var remoteEnvironment = instantiatedComponents.Single(e => inputPortConfig.RemoteId == e.Configuration.Id);
            var remoteOutputMetadata = remoteEnvironment.FindPortMetadata(inputPortConfig.RemotePort);
            Debug.Assert(remoteOutputMetadata.Direction == PortDirection.Output);
            var getProducerFunc = typeof(HelperExtensions)
                .GetMethod(nameof(HelperExtensions.GetProducer))
                .MakeGenericMethod(consumerCollectionType);//Note: not producerCollectionType here, so that type conversion is applied if needed
            dynamic producer = getProducerFunc.Invoke(null, new object[] { remoteEnvironment, inputPortConfig.RemotePort });
            Operators.PipeTo(producer, consumer, inputPortConfig.DeliveryPolicy);

            return instance;
        }

        /// <summary>
        /// This method is called to initialize an instance. After the instance is returned, connections of ports will be added.
        /// </summary>
        /// <param name="pipeline">The pipeline will be connected to.</param>
        /// <param name="serviceProvider">A <see cref="IServiceProvider"/> can be used as needed. Can be <see cref="null"/>.</param>
        /// <returns>An instance of the component initialized using the current configuration.</returns>
        protected abstract object Instantiate<T>(Pipeline pipeline, IServiceProvider serviceProvider);

        #region Helpers

        private static (dynamic Producer, Type DataType, IReadOnlyList<Type> ElementTypes) AnalyzeProducer(dynamic producer) {
            var dataType = HelperExtensions.GetProducerResultType(producer);
            var elementTypes = HelperExtensions.FindElementTypesOfCollectionType(dataType);
            return (producer, dataType, elementTypes);
        }
        #endregion
    }
}
