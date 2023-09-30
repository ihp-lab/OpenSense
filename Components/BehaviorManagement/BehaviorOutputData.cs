using System;
using System.Diagnostics;

namespace OpenSense.Components.BehaviorManagement {
    public readonly struct BehaviorOutputData {

        public IPortMetadata Port { get; }

        public Type DataType { get; }

        public object? Data { get; }

        public DateTime OriginatingTime { get; }

        public BehaviorOutputData(IPortMetadata portMetadata, Type dataType, object? data, DateTime originatingTime) {
            Debug.Assert(originatingTime.Kind == DateTimeKind.Utc);
            Port = portMetadata;
            DataType = dataType;
            Data = data;
            OriginatingTime = originatingTime;
        }

        #region Helpers

        #endregion
    }
}
