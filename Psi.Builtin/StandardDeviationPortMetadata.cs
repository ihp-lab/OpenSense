using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using Microsoft.Psi;
using OpenSense.Component.Contract;

namespace OpenSense.Component.Psi {
    public sealed class StandardDeviationPortMetadata : OperatorPortMetadata {

        /** Officially supported overloads.
         * Find overloads with arrays as input and a relative time is not required.
         */
        private static readonly Type[] ValidInputArrayTypes = typeof(Operators)
            .GetMethods(BindingFlags.Public | BindingFlags.Static)
            .Where(m => m.Name == nameof(Operators.Std))
            .Select(m => m.GetParameters())
            .Where(p => p.Length == 2) 
            .Where(p => p[1].ParameterType.IsGenericType)
            .Where(p => p[1].ParameterType.GetGenericTypeDefinition() == typeof(DeliveryPolicy<>))
            .Select(p => p[0].ParameterType)
            .Where(t => t.IsGenericType)
            .Where(t => t.GetGenericTypeDefinition() == typeof(IProducer<>))
            .Select(t => t.GetGenericArguments()[0])
            .Where(t => t.IsArray)
            .ToArray()
            ;

        public StandardDeviationPortMetadata(string name, PortDirection direction, string description = "") : base(name, direction, description) {
        }

        public override bool CanConnectDataType(Type remoteEndPointDataType, IList<Type> localOtherDirectionPortsDataTypes, IList<Type> localSameDirectionPortsDataTypes) {
            switch (Direction) {
                case PortDirection.Input:
                    Debug.Assert(ValidInputArrayTypes.Length > 0);
                    var result = ValidInputArrayTypes.Contains(remoteEndPointDataType);
                    return result;
                default:
                    return base.CanConnectDataType(remoteEndPointDataType, localOtherDirectionPortsDataTypes, localSameDirectionPortsDataTypes);
            }
        }

        public override Type GetTransmissionDataType(Type remoteEndPointDataType, IList<Type> localOtherDirectionPortsDataTypes, IList<Type> localSameDirectionPortsDataTypes) {
            switch (Direction) {
                case PortDirection.Input:
                    return remoteEndPointDataType;
                case PortDirection.Output:
                    if (localOtherDirectionPortsDataTypes.Count != 1) {
                        return null;
                    }
                    var inputType = localOtherDirectionPortsDataTypes.Single();
                    if (inputType is null) {
                        return null;
                    }
                    if (!inputType.IsArray) {
                        return null;
                    }
                    var result = inputType.GetElementType();
                    return result;
                default:
                    throw new InvalidOperationException();
            }
        }
    }
}
