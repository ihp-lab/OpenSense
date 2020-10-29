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

        #region data type finder
        public static Type FindInputPortDataType(this ComponentConfiguration config, IPortMetadata portMetadata, IReadOnlyList<ComponentConfiguration> configs) {
            var dataType = portMetadata.GetTransmissionDataType(null, Array.Empty<Type>(), Array.Empty<Type>());
            if (dataType is null) {
                var i = config.Inputs.FirstOrDefault(c => Equals(c.LocalPort.Identifier, portMetadata.Identifier));
                if (i is null) {
                    return null;
                }
                foreach (var other in configs) {
                    if (Equals(i.RemoteId, other.Id)) {
                        var oMetadata = other.GetMetadata().FindPortMetadata(i.RemotePort);
                        var otherInput = FindInputPortDataTypes(other, configs);
                        var otherOutput = FindOutputPortDataTypes(other, configs);
                        var otherEnd = oMetadata.GetTransmissionDataType(null, otherInput, otherOutput);
                        if (otherEnd != null) {
                            var selfOutput = FindOutputPortDataTypes(config, configs);
                            dataType = portMetadata.GetTransmissionDataType(otherEnd, selfOutput, Array.Empty<Type>());
                            if (dataType != null) {
                                goto jump;
                            }
                            var selfInput = FindInputPortDataTypes(config, configs, portMetadata);
                            dataType = portMetadata.GetTransmissionDataType(otherEnd, selfOutput, selfInput);
                            if (dataType != null) {
                                goto jump;
                            }
                        }

                    }
                }
jump:;
            }
            return dataType;
        }

        public static IList<Type> FindInputPortDataTypes(this ComponentConfiguration config, IReadOnlyList<ComponentConfiguration> configs, IPortMetadata except = null) {
            var inputPorts = config.GetMetadata().InputPorts().ToArray();
            var result = new Type[inputPorts.Length];
            var idx = 0;
            foreach (var iMetadata in inputPorts) {
                Type dataType;
                if (except != null && Equals(except.Identifier, iMetadata.Identifier)) {
                    dataType = null;
                } else {
                    dataType = FindInputPortDataType(config, iMetadata, configs);
                }
                result[idx] = dataType;
                idx++;
            }
            return result;
        }

        public static Type FindOutputPortDataType(this ComponentConfiguration config, IPortMetadata portMetadata, IReadOnlyList<ComponentConfiguration> configs) {
            var dataType = portMetadata.GetTransmissionDataType(null, Array.Empty<Type>(), Array.Empty<Type>());
            if (dataType is null) {
                foreach (var other in configs) {
                    foreach (var i in other.Inputs) {
                        var iMetadata = other.GetMetadata().FindPortMetadata(i.LocalPort);
                        if (Equals(i.RemoteId, config.Id) && Equals(i.RemotePort.Identifier, portMetadata.Identifier)) {
                            var otherOutput = FindOutputPortDataTypes(other, configs);
                            var otherInput = FindInputPortDataTypes(other, configs);
                            var otherEnd = iMetadata.GetTransmissionDataType(null, otherOutput, otherInput);
                            if (otherEnd != null) {
                                var selfInput = FindInputPortDataTypes(config, configs);
                                dataType = portMetadata.GetTransmissionDataType(otherEnd, selfInput, Array.Empty<Type>());
                                if (dataType != null) {
                                    goto jump;
                                }
                                var selfOutput = FindOutputPortDataTypes(config, configs, portMetadata);
                                dataType = portMetadata.GetTransmissionDataType(otherEnd, selfInput, selfOutput);
                                if (dataType != null) {
                                    goto jump;
                                }
                            }
                        }
                    }
                }
jump:;
            }
            return dataType;
        }

        public static IList<Type> FindOutputPortDataTypes(this ComponentConfiguration config, IReadOnlyList<ComponentConfiguration> configs, IPortMetadata except = null) {
            var outputPorts = config.GetMetadata().OutputPorts().ToArray();
            var result = new Type[outputPorts.Length];
            var idx = 0;
            foreach (var oMetadata in outputPorts) {
                Type dataType;
                if (except != null && Equals(except.Identifier, oMetadata.Identifier)) {
                    dataType = null;
                } else {
                    dataType = FindOutputPortDataType(config, oMetadata, configs);
                }
                result[idx] = dataType;
                idx++;
            }
            return result;
        }
        #endregion
    }
}
