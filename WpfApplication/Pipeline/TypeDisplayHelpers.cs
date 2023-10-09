using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using OpenSense.Components;

namespace OpenSense.WPF.Pipeline {
    internal static class TypeDisplayHelpers {

        public static string FindInputDataTypeName(ComponentConfiguration configuration, IPortMetadata inputMetadata, IReadOnlyList<ComponentConfiguration> configurations) {
            Debug.Assert(inputMetadata.Direction == PortDirection.Input);
            var localOutputs = configuration.FindOutputPortDataTypes(configurations);
            var localInputs = configuration.FindInputPortDataTypes(configurations, exclude: inputMetadata);
            string result;
            if (inputMetadata.CanConnectDataType(null, localOutputs, localInputs)) {
                result = "Any Type";
            } else {
                var inputPortDataType = inputMetadata.GetTransmissionDataType(null, localOutputs, localInputs);
                if (inputPortDataType is null) {
                    result = "Type Unknown";
                } else {
                    result = GetCSharpStyleTypeName(inputPortDataType);
                }
            }
            return result;
        }

        public static string FindOutputDataTypeName(ComponentConfiguration configuration, IPortMetadata outputMetadata, IReadOnlyList<ComponentConfiguration> configurations) {
            Debug.Assert(outputMetadata.Direction == PortDirection.Output);
            var localInputs = configuration.FindInputPortDataTypes(configurations);
            var localOutputs = configuration.FindOutputPortDataTypes(configurations, exclude: outputMetadata);
            string result;
            if (outputMetadata.CanConnectDataType(null, localInputs, localOutputs)) {
                result = "Any Type";
            } else {
                var outputPortDataType = outputMetadata.GetTransmissionDataType(null, localInputs, localOutputs);
                if (outputPortDataType is null) {
                    result = "Type Unknown";
                } else {
                    result = GetCSharpStyleTypeName(outputPortDataType);
                }
            }
            return result;
        }

        private static string GetCSharpStyleTypeName(Type type) {
            if (TypeMappings.TryGetValue(type, out var name)) {
                return name;
            }
            if (type.IsArray) {
                return $"{GetCSharpStyleTypeName(type.GetElementType())}[]";
            }
            name = $"{type.Namespace}.{type.Name}";
            if (!type.IsGenericType) {
                return name;
            }
            var sb = new StringBuilder();
            var innerText = string.Join(", ", type.GetGenericArguments().Select(GetCSharpStyleTypeName));
            if (name.StartsWith("System.ValueTuple`")) {
                sb.Append('(');
                sb.Append(innerText);
                sb.Append(')');
            } else {
                sb.Append(name.Substring(0, name.IndexOf('`')));
                sb.Append('<');
                sb.Append(innerText);
                sb.Append('>');
            }
            var result = sb.ToString();
            return result;
        }

        private static readonly Dictionary<Type, string> TypeMappings = new Dictionary<Type, string>() {
            { typeof(object), "object" },
            { typeof(string), "string" },
            { typeof(bool), "bool" },
            { typeof(float), "float" },
            { typeof(double), "double" },
            { typeof(int), "int" },
            { typeof(uint), "uint" },
            { typeof(long), "long" },
            { typeof(ulong), "ulong" },
            { typeof(short), "short" },
            { typeof(ushort), "ushort" },

        };
    }
}
