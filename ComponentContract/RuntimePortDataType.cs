#nullable enable

using System;

namespace OpenSense.Components.Contract {
    public sealed class RuntimePortDataType : IEquatable<RuntimePortDataType> {

        public IPortMetadata Metadata { get; }

        public Type? Type { get; }

        public RuntimePortDataType(IPortMetadata metadata, Type? type) {
            Metadata = metadata;
            Type = type;
        }

        #region IEquatable
        public bool Equals(RuntimePortDataType? other) {
            if (other is null) {
                return false;
            }
            if (ReferenceEquals(this, other)) {
                return true;
            }
            if (!Equals(Metadata, other.Metadata)) {
                return false;
            }
            if (!Equals(Type, other.Type)) {
                return false;
            }
            return true;
        }

        public override bool Equals(object obj) =>
            obj is RuntimePortDataType other && Equals(other);

        public override int GetHashCode() =>
            HashCode.Combine(Metadata, Type); 
        #endregion
    }
}
