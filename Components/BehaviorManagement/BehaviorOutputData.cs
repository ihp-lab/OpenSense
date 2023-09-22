using System;
using System.Diagnostics;
using Microsoft.Psi;

namespace OpenSense.Components.BehaviorManagement {
    public readonly struct BehaviorOutputData {

        public IPortMetadata Port { get; }

        public Type Type { get; }

        public object? Data { get; }

        public Envelope Envelope { get; }

        public BehaviorOutputData(IPortMetadata portMetadata, Type type, object? data, Envelope envelope) {
            Debug.Assert(envelope.OriginatingTime.Kind == DateTimeKind.Utc);
            Port = portMetadata;
            Type = type;
            Data = data;
            Envelope = envelope;
        }

        #region Helpers

        #endregion
    }
}
