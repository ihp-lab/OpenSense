using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenSense.Component.Contract;

namespace OpenSense.Wpf.Pipeline {
    public static class PortDataTypeFinder {
        public static Type FindInputPortDataType(ComponentConfiguration config, IPortMetadata portMetadata, IReadOnlyList<ComponentConfiguration> configs) {
            var dataType = portMetadata.DataType(null, Array.Empty<Type>(), Array.Empty<Type>());
            if (dataType is null) {
                var i = config.Inputs.FirstOrDefault(c => Equals(c.LocalPort.Identifier, portMetadata.Identifier));
                foreach (var other in configs) {
                    if (Equals(i.RemoteId, other.Id)) {
                        var oMetadata = other.GetMetadata().FindPortMetadata(i.RemotePort);
                        var otherInput = FindInputPortDataTypes(other, configs);
                        var otherOutput = FindOutputPortDataTypes(other, configs);
                        var otherEnd = oMetadata.DataType(null, otherInput, otherOutput);
                        if (otherEnd != null) {
                            var selfOutput = FindOutputPortDataTypes(config, configs);
                            dataType = portMetadata.DataType(otherEnd, selfOutput, Array.Empty<Type>());
                            if (dataType != null) {
                                goto jump;
                            }
                            var selfInput = FindInputPortDataTypes(config, configs, portMetadata);
                            dataType = portMetadata.DataType(otherEnd, selfOutput, selfInput);
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

        public static IList<Type> FindInputPortDataTypes(ComponentConfiguration config, IReadOnlyList<ComponentConfiguration> configs, IPortMetadata except = null) {
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

        public static Type FindOutputPortDataType(ComponentConfiguration config, IPortMetadata portMetadata, IReadOnlyList<ComponentConfiguration> configs) {
            var dataType = portMetadata.DataType(null, Array.Empty<Type>(), Array.Empty<Type>());
            if (dataType is null) {
                foreach (var other in configs) {
                    foreach (var i in other.Inputs) {
                        var iMetadata = other.GetMetadata().FindPortMetadata(i.LocalPort);
                        if (Equals(i.RemoteId, config.Id) && Equals(i.RemotePort.Identifier, portMetadata.Identifier)) {
                            var otherOutput = FindOutputPortDataTypes(other, configs);
                            var otherInput = FindInputPortDataTypes(other, configs);
                            var otherEnd = iMetadata.DataType(null, otherOutput, otherInput);
                            if (otherEnd != null) {
                                var selfInput = FindInputPortDataTypes(config, configs);
                                dataType = portMetadata.DataType(otherEnd, selfInput, Array.Empty<Type>());
                                if (dataType != null) {
                                    goto jump;
                                }
                                var selfOutput = FindOutputPortDataTypes(config, configs, portMetadata);
                                dataType = portMetadata.DataType(otherEnd, selfInput, selfOutput);
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

        public static IList<Type> FindOutputPortDataTypes(ComponentConfiguration config, IReadOnlyList<ComponentConfiguration> configs, IPortMetadata except = null) {
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
    }
}
