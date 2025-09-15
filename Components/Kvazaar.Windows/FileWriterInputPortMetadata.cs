using System;
using System.Collections.Generic;
using System.Reflection;
using Microsoft.Psi;
using Microsoft.Psi.Imaging;

namespace OpenSense.Components.Kvazaar {
    internal sealed class FileWriterInputPortMetadata : IPortMetadata {
        private readonly System.Reflection.PropertyInfo _propertyInfo;

        public FileWriterInputPortMetadata() {
            _propertyInfo = typeof(FileWriter<>).GetProperty(nameof(FileWriter<ImageBase>.In))!;
        }

        public object Identifier => Name;

        public string Name => _propertyInfo.Name;

        public string? Description => "[Required] The input image stream. Must be 16-bit grayscale images.";

        public PortDirection Direction => PortDirection.Input;

        public PortAggregation Aggregation => PortAggregation.Object;

        public bool CanConnectDataType(RuntimePortDataType? remoteEndPointDataType, IReadOnlyList<RuntimePortDataType> localOtherDirectionPortsDataTypes, IReadOnlyList<RuntimePortDataType> localSameDirectionPortsDataTypes) {
            if (remoteEndPointDataType?.Type is null) {
                return false;
            }

            // Check if the remote type is Shared<T> where T : ImageBase
            var remoteType = remoteEndPointDataType.Type;
            if (!remoteType.IsGenericType || remoteType.GetGenericTypeDefinition() != typeof(Shared<>)) {
                return false;
            }

            var innerType = remoteType.GetGenericArguments()[0];
            return typeof(ImageBase).IsAssignableFrom(innerType);
        }

        public Type? GetTransmissionDataType(RuntimePortDataType? remoteEndPointDataType, IReadOnlyList<RuntimePortDataType> localOtherDirectionPortsDataTypes, IReadOnlyList<RuntimePortDataType> localSameDirectionPortsDataTypes) {
            if (remoteEndPointDataType?.Type is null) {
                return null;
            }

            var remoteType = remoteEndPointDataType.Type;
            if (!remoteType.IsGenericType || remoteType.GetGenericTypeDefinition() != typeof(Shared<>)) {
                return null;
            }

            var innerType = remoteType.GetGenericArguments()[0];
            if (!typeof(ImageBase).IsAssignableFrom(innerType)) {
                return null;
            }

            return remoteType;
        }

        public PropertyInfo GetConstructedPropertyInfo(params Type[] typeArguments) {
            var declaringType = _propertyInfo.DeclaringType!
                .GetGenericTypeDefinition()
                .MakeGenericType(typeArguments);
            var result = declaringType.GetProperty(_propertyInfo.Name, System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public);
            return result!;
        }
    }
}
