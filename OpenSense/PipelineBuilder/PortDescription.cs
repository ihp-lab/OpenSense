using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using Microsoft.Psi;
using Microsoft.Psi.Data;

namespace OpenSense.PipelineBuilder {

    internal struct Any { }

    public enum PortType {
        Input,
        Output,
    }

    public abstract class PortDescription{

        public string Description { get; set; }

        public abstract string Name { get; }

        public virtual bool IsList => false;

        public virtual bool IsDictionary => false;
    }

    public class StaticPortDescription : PortDescription {
        
        public PropertyInfo Property { get; set; }

        public override string Name => Property.Name;

        public override bool IsList => Property.PropertyType.GetInterfaces().Any(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IReadOnlyList<>));

        public override bool IsDictionary => Property.PropertyType.GetInterfaces().Any(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IReadOnlyDictionary<,>));

        public Type DataType { 
            get {
                var propType = Property.PropertyType;
                if (IsList) {
                    return GetDataTypeFromProducerType(propType.GetGenericArguments().Single());
                }
                if (IsDictionary) {
                    Debug.Assert(propType.GetGenericArguments()[0] == typeof(string));
                    return GetDataTypeFromProducerType(propType.GetGenericArguments()[1]);
                }
                return GetDataTypeFromProducerType(propType);
            }
        }

        private static Type GetDataTypeFromProducerType(Type propType) {
            return propType.GetGenericArguments().Single();
        }
    }

    public class VirtualPortDescription : PortDescription {

        public PortType PortType { get; set; }

        public string VirtualName { get; set; }

        public override string Name => VirtualName;

        public Type DataType(InstanceConfiguration instance, IList<InstanceConfiguration> instances) {
            var filtered = instances.ToList();
            filtered.Remove(instance);
            #region helper func
            Type getInputDataType(InputConfiguration inConfig) {
                var remote = filtered.SingleOrDefault(r => r.Guid == inConfig.Remote);
                if (remote is null) {
                    return null;
                }
                var remoteDesc = ConfigurationManager.Description(remote);
                var remoteOutDesc = remoteDesc.Outputs.Single(o => o.Name == inConfig.Output.PropertyName);
                switch (remoteOutDesc) {
                    case VirtualPortDescription vRemoteOutDesc:
                        return vRemoteOutDesc.DataType(remote, filtered);
                    case StaticPortDescription sRemoteOutDesc:
                        return sRemoteOutDesc.DataType;
                    default:
                        throw new InvalidOperationException();
                }
            }
            #endregion
            switch (PortType) {
                case PortType.Input: // determin by remote output type
                    var inConfig = instance.Inputs.Single(c => c.Input.PropertyName == Name);
                    return getInputDataType(inConfig);
                case PortType.Output:
                    switch (instance) {
                        case OperatorConfiguration opConfig:
                            var inputTypes = instance.Inputs.Select(i => getInputDataType(i)).ToArray();
                            if (inputTypes.Any(t => t is null)) {
                                return null;
                            }
                            return opConfig.OutputDataType(inputTypes);
                        case StreamReaderConfiguration inporterConfig:
                            return typeof(Any);
                        default:
                            throw new InvalidOperationException();
                    }
                default:
                    throw new InvalidOperationException();
            }
        }
    }
}
