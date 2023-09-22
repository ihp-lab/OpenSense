using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Microsoft.Psi;
using static OpenSense.Components.BehaviorManagement.BehaviorInputData;

namespace OpenSense.Components.BehaviorManagement {
    public readonly struct BehaviorInputData : IReadOnlyList<DataItem> {

        private readonly IReadOnlyList<DataItem> _data;

        public IPortMetadata Port { get; }

        public Type DataType { get; }

        public BehaviorInputData(IPortMetadata port, Type dataType, IReadOnlyList<DataItem> data) {
            Debug.Assert(data.All(m => m.Envelope.OriginatingTime.Kind == DateTimeKind.Utc));
            Debug.Assert(data.Select(m => m.Envelope.OriginatingTime).Prepend(DateTime.MinValue).Zip(data.Select(m => m.Envelope.OriginatingTime).Append(DateTime.MaxValue), ValueTuple.Create).All(t => t.Item1 < t.Item2));
            Port = port;
            DataType = dataType;
            _data = data;
        }

        #region Helpers
        public IEnumerable<DataItem> Window(DateTime time, TimeSpan window) {
            Debug.Assert(time.Kind == DateTimeKind.Utc);
            Debug.Assert(window >= TimeSpan.Zero);
            var start = time - window;
            foreach (var item in _data) {
                if (item.Envelope.OriginatingTime >= start) {
                    yield return item;
                }
            }
        }
        #endregion

        #region IReadOnlyList

        public int Count => _data.Count;

        public DataItem this[int index] => _data[index];

        public IEnumerator<DataItem> GetEnumerator() => _data.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => _data.GetEnumerator();

        #endregion

        #region Classes
        public readonly struct DataItem {

            public object? Data { get; }

            public Envelope Envelope { get; }

            public DataItem(object? data, Envelope envelope) {
                Data = data;
                Envelope = envelope;
            }

            public static implicit operator DataItem(Message<object?> message) => new DataItem(message.Data, new Envelope(message.OriginatingTime, message.CreationTime, message.SourceId, message.SequenceId));

            public static implicit operator Message<object?>(DataItem item) => new Message<object?>(item.Data, item.Envelope.OriginatingTime, item.Envelope.CreationTime, item.Envelope.SourceId, item.Envelope.SequenceId);
        }
        #endregion
    }
}
