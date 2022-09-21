using System;

namespace OpenSense.Component.Contract.PortDataTypeInferences {
    internal sealed class InferenceExclusionItem : IEquatable<InferenceExclusionItem> {

        private readonly ComponentConfiguration _component;

        private readonly IPortMetadata _port;

        public ComponentConfiguration Component => _component;

        public IPortMetadata Port => _port;

        public InferenceExclusionItem(ComponentConfiguration component, IPortMetadata port) {
            _component = component;
            _port = port;
        }

        #region IEquatable
        public bool Equals(InferenceExclusionItem other) {
            return Component == other.Component && Port == other.Port;
        }

        public override bool Equals(object obj) => obj is InferenceExclusionItem other ? Equals(other) : false;

        public static bool operator ==(InferenceExclusionItem left, InferenceExclusionItem right) => Equals(left, right);

        public static bool operator !=(InferenceExclusionItem left, InferenceExclusionItem right) => !Equals(left, right);

        public override int GetHashCode() => HashCode.Combine(Component, Port);
        #endregion
    }
}
