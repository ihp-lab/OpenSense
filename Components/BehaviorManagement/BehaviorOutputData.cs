using System;
using System.Diagnostics;
using Microsoft.Psi;

namespace OpenSense.Components.BehaviorManagement {
    public readonly struct BehaviorOutputData {

        public IPortMetadata Port { get; }

        public Type DataType { get; }

        public object? Data { get; }

        public Envelope Envelope { get; }

        public BehaviorOutputData(IPortMetadata portMetadata, Type dataType, object? data, Envelope envelope) {
            Debug.Assert(envelope.OriginatingTime.Kind == DateTimeKind.Utc);
            Port = portMetadata;
            DataType = dataType;
            Data = data;
            Envelope = envelope;
        }

        #region Helpers

        #endregion
    }
}
