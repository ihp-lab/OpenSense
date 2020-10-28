using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Microsoft.Psi;

namespace OpenSense.Component.Contract {
    public static class HelperExtensions {

        public static IEnumerable<IPortMetadata> InputPorts(this IComponentMetadata componentMetadata) {
            return componentMetadata.Ports.Where(p => p.Direction == PortDirection.Input);
        }

        public static IEnumerable<IPortMetadata> OutputPorts(this IComponentMetadata componentMetadata) {
            return componentMetadata.Ports.Where(p => p.Direction == PortDirection.Output);
        }

        public static IPortMetadata FindPortMetadata(this IComponentMetadata componentMetadata, PortConfiguration port) {
            return componentMetadata.Ports.Single(p => Equals(p.Identifier, port.Identifier));
        }

        public static IProducer<T> GetOutputProducerOfStaticPort<T>(this IComponentMetadata componentMetadata, object instance, PortConfiguration portConfiguration) {
            var portMetadata = componentMetadata.FindPortMetadata(portConfiguration);
            Debug.Assert(portMetadata.Direction == PortDirection.Output);
            Debug.Assert(portMetadata is StaticPortMetadata);
            var portStaticMetadata = (StaticPortMetadata)portMetadata;
            return portStaticMetadata.GetStaticOutputProducer<T>(portConfiguration, instance);
        }

        public static IProducer<T> GetOutputProducer<T>(this ComponentEnvironment componentEnvironment, PortConfiguration port) {
            return componentEnvironment.Configuration.GetMetadata().GetOutputProducer<T>(componentEnvironment.Instance, port);
        }

        private static dynamic GetStaticPort(this object instance, StaticPortMetadata portMetadata, PortConfiguration portConfiguration) {
            Debug.Assert(Equals(portMetadata.Identifier, portConfiguration.Identifier));
            dynamic prop = portMetadata.Property.GetValue(instance);
            switch (portMetadata.Aggregation) {
                case PortAggregation.Object:
                    return prop;
                case PortAggregation.List:
                    return prop[(int)portConfiguration.Index];
                case PortAggregation.Dictionary:
                    return prop[(string)portConfiguration.Index];
                default:
                    throw new InvalidOperationException();
            }
        }

        public static IProducer<T> GetStaticOutputProducer<T>(this StaticPortMetadata portMetadata, PortConfiguration portConfiguration, object instance) {
            dynamic obj = GetStaticPort(instance, portMetadata, portConfiguration);
            return (IProducer<T>)obj;
        }

        public static IConsumer<T> GetStaticInputConsumer<T>(this StaticPortMetadata portMetadata, PortConfiguration portConfiguration, object instance) {
            dynamic obj = GetStaticPort(instance, portMetadata, portConfiguration);
            return (IConsumer<T>)obj;
        }

        public static void ConnectAllStaticInputs(this ComponentConfiguration componentConfiguration, object instance, IReadOnlyList<ComponentEnvironment> instantiatedComponents) {
            foreach (var inputConfig in componentConfiguration.Inputs) {
                var inputMetadata = componentConfiguration.GetMetadata().FindPortMetadata(inputConfig.LocalPort);
                Debug.Assert(inputMetadata.Direction == PortDirection.Input);
                var inputStaticMetadata = inputMetadata as StaticPortMetadata;
                if (inputStaticMetadata is null) {
                    continue;
                }
                var dataType = inputStaticMetadata.DataType;
                var getConsumerFunc = typeof(HelperExtensions).GetMethod(nameof(GetStaticInputConsumer)).MakeGenericMethod(dataType);
                dynamic consumer = getConsumerFunc.Invoke(null, new object[] { inputStaticMetadata, inputConfig.LocalPort, instance });

                var remoteEnvironment = instantiatedComponents.Single(e => Equals(inputConfig.RemoteId, e.Configuration.Id));
                var remoteOutputMetadata = remoteEnvironment.Configuration.GetMetadata().FindPortMetadata(inputConfig.RemotePort);
                Debug.Assert(remoteOutputMetadata.Direction == PortDirection.Output);
                var getProducerFunc = typeof(HelperExtensions).GetMethod(nameof(GetOutputProducer)).MakeGenericMethod(dataType);
                dynamic producer = getProducerFunc.Invoke(null, new object[] { remoteEnvironment, inputConfig.RemotePort});

                Operators.PipeTo(producer, consumer, inputConfig.DeliveryPolicy);
            }
        }
    }
}
