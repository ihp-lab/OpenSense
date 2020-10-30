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

        public static IProducer<T> GetStaticPortOutputProducer<T>(this IComponentMetadata componentMetadata, object instance, PortConfiguration portConfiguration) {
            var portMetadata = componentMetadata.FindPortMetadata(portConfiguration);
            Debug.Assert(portMetadata.Direction == PortDirection.Output);
            Debug.Assert(portMetadata is StaticPortMetadata);
            var portStaticMetadata = (StaticPortMetadata)portMetadata;
            return portStaticMetadata.GetStaticOutputProducer<T>(portConfiguration, instance);
        }

        public static object GetOutputConnector<T>(this ComponentEnvironment componentEnvironment, PortConfiguration port) {
            return componentEnvironment.Configuration.GetMetadata().GetOutputConnector<T>(componentEnvironment.Instance, port);
        }

        public static IProducer<T> GetOutputProducer<T>(this ComponentEnvironment componentEnvironment, PortConfiguration port) {
            var connector = GetOutputConnector<T>(componentEnvironment, port);
            Debug.Assert(typeof(IProducer<T>).IsAssignableFrom(connector.GetType()));
            return (IProducer<T>)connector;
        }

        private static dynamic GetStaticConnector(this object instance, StaticPortMetadata portMetadata, PortConfiguration portConfiguration) {
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
            dynamic obj = GetStaticConnector(instance, portMetadata, portConfiguration);
            return (IProducer<T>)obj;
        }

        public static IConsumer<T> GetStaticInputConsumer<T>(this StaticPortMetadata portMetadata, PortConfiguration portConfiguration, object instance) {
            dynamic obj = GetStaticConnector(instance, portMetadata, portConfiguration);
            return (IConsumer<T>)obj;
        }

        /// <summary>
        /// Requirement: all local input metadata should be StaticPortMetadata, and all remote output connectors should be IProducer<T>
        /// </summary>
        /// <param name="componentConfiguration"></param>
        /// <param name="instance"></param>
        /// <param name="instantiatedComponents"></param>
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
                Debug.Assert(inputStaticMetadata.CanConnectConnectorType(remoteOutputMetadata.ConnectorType));
                var getProducerFunc = typeof(HelperExtensions).GetMethod(nameof(GetOutputProducer)).MakeGenericMethod(dataType);
                dynamic producer = getProducerFunc.Invoke(null, new object[] { remoteEnvironment, inputConfig.RemotePort});

                Operators.PipeTo(producer, consumer, inputConfig.DeliveryPolicy);
            }
        }

        #region data type finder
        public static Type FindInputPortDataType(this ComponentConfiguration config, IPortMetadata portMetadata, IReadOnlyList<ComponentConfiguration> configs, params Tuple<ComponentConfiguration, IPortMetadata>[] exclude) {
            var dataType = portMetadata.GetTransmissionDataType(null, Array.Empty<Type>(), Array.Empty<Type>());
            if (dataType is null) {
                var i = config.Inputs.FirstOrDefault(c => Equals(c.LocalPort.Identifier, portMetadata.Identifier));
                if (i is null) {
                    return null;
                }
                foreach (var other in configs) {
                    if (Equals(i.RemoteId, other.Id)) {
                        var newExclude = new Tuple<ComponentConfiguration, IPortMetadata>[exclude.Length + 1];
                        Array.Copy(exclude, newExclude, exclude.Length);
                        var oMetadata = other.GetMetadata().FindPortMetadata(i.RemotePort);
                        newExclude[exclude.Length] = new Tuple<ComponentConfiguration, IPortMetadata>(other, oMetadata);
                        var otherInput = FindInputPortDataTypes(other, configs, newExclude);
                        var otherOutput = FindOutputPortDataTypes(other, configs, newExclude);
                        var otherEnd = oMetadata.GetTransmissionDataType(null, otherInput, otherOutput);
                        if (otherEnd != null) {
                            var selfOutput = FindOutputPortDataTypes(config, configs);
                            dataType = portMetadata.GetTransmissionDataType(otherEnd, selfOutput, Array.Empty<Type>());
                            if (dataType != null) {
                                goto jump;
                            }
                            newExclude[exclude.Length] = new Tuple<ComponentConfiguration, IPortMetadata>(config, portMetadata);
                            var selfInput = FindInputPortDataTypes(config, configs, newExclude);
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

        public static IList<Type> FindInputPortDataTypes(this ComponentConfiguration config, IReadOnlyList<ComponentConfiguration> configs, params Tuple<ComponentConfiguration, IPortMetadata>[] exclude) {
            var inputPorts = config.GetMetadata().InputPorts().ToArray();
            var result = new Type[inputPorts.Length];
            var idx = 0;
            foreach (var iMetadata in inputPorts) {
                Type dataType;
                if (exclude != null && exclude.Any(ex => ex.Item1.Id == config.Id && Equals(ex.Item2.Identifier, iMetadata.Identifier))) {
                    dataType = null;
                } else {
                    dataType = FindInputPortDataType(config, iMetadata, configs, exclude);
                }
                result[idx] = dataType;
                idx++;
            }
            return result;
        }

        public static IList<Type> FindInputPortDataTypes(this ComponentConfiguration config, IReadOnlyList<ComponentConfiguration> configs, IPortMetadata exclude) {
            return FindInputPortDataTypes(config, configs, new Tuple<ComponentConfiguration, IPortMetadata>(config, exclude));
        }

        public static Type FindOutputPortDataType(this ComponentConfiguration config, IPortMetadata portMetadata, IReadOnlyList<ComponentConfiguration> configs, params Tuple<ComponentConfiguration, IPortMetadata>[] exclude) {
            var dataType = portMetadata.GetTransmissionDataType(null, Array.Empty<Type>(), Array.Empty<Type>());
            if (dataType is null) {
                foreach (var other in configs) {
                    foreach (var i in other.Inputs) {
                        var iMetadata = other.GetMetadata().FindPortMetadata(i.LocalPort);
                        if (Equals(i.RemoteId, config.Id) && Equals(i.RemotePort.Identifier, portMetadata.Identifier)) {
                            var newExclude = new Tuple<ComponentConfiguration, IPortMetadata>[exclude.Length + 1];
                            Array.Copy(exclude, newExclude, exclude.Length);
                            newExclude[exclude.Length] = new Tuple<ComponentConfiguration, IPortMetadata>(other, iMetadata);
                            var otherOutput = FindOutputPortDataTypes(other, configs, newExclude);
                            var otherInput = FindInputPortDataTypes(other, configs, newExclude);
                            var otherEnd = iMetadata.GetTransmissionDataType(null, otherOutput, otherInput);
                            if (otherEnd != null) {
                                var selfInput = FindInputPortDataTypes(config, configs);
                                dataType = portMetadata.GetTransmissionDataType(otherEnd, selfInput, Array.Empty<Type>());
                                if (dataType != null) {
                                    goto jump;
                                }
                                newExclude[exclude.Length] = new Tuple<ComponentConfiguration, IPortMetadata>(config, portMetadata);
                                var selfOutput = FindOutputPortDataTypes(config, configs, newExclude);
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

        public static IList<Type> FindOutputPortDataTypes(this ComponentConfiguration config, IReadOnlyList<ComponentConfiguration> configs, params Tuple<ComponentConfiguration, IPortMetadata>[] exclude) {
            var outputPorts = config.GetMetadata().OutputPorts().ToArray();
            var result = new Type[outputPorts.Length];
            var idx = 0;
            foreach (var oMetadata in outputPorts) {
                Type dataType;
                if (exclude != null && exclude.Any(ex => ex.Item1.Id == config.Id && Equals(ex.Item2.Identifier, oMetadata.Identifier))) {
                    dataType = null;
                } else {
                    dataType = FindOutputPortDataType(config, oMetadata, configs, exclude);
                }
                result[idx] = dataType;
                idx++;
            }
            return result;
        }

        public static IList<Type> FindOutputPortDataTypes(this ComponentConfiguration config, IReadOnlyList<ComponentConfiguration> configs, IPortMetadata exclude) {
            return FindOutputPortDataTypes(config, configs, new Tuple<ComponentConfiguration, IPortMetadata>(config, exclude));
        }
        #endregion
    }
}
