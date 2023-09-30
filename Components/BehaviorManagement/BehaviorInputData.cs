using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Microsoft.Psi;

namespace OpenSense.Components.BehaviorManagement {
    public readonly struct BehaviorInputData : IReadOnlyList<Message<object?>> {

        private readonly IReadOnlyList<Message<object?>> _data;

        public IPortMetadata Port { get; }

        public Type DataType { get; }

        public BehaviorInputData(IPortMetadata port, Type dataType, IReadOnlyList<Message<object?>> data) {
            Debug.Assert(data.All(m => m.OriginatingTime.Kind == DateTimeKind.Utc));
            Debug.Assert(data.Select(m => m.OriginatingTime).Prepend(DateTime.MinValue).Zip(data.Select(m => m.OriginatingTime).Append(DateTime.MaxValue), ValueTuple.Create).All(t => t.Item1 < t.Item2));
            Port = port;
            DataType = dataType;
            _data = data;
        }

        #region Helpers
        public IEnumerable<Message<object?>> Window(DateTime time, TimeSpan window) {
            Debug.Assert(time.Kind == DateTimeKind.Utc);
            Debug.Assert(window >= TimeSpan.Zero);
            var start = time - window;
            foreach (var item in _data) {
                if (item.OriginatingTime >= start) {
                    yield return item;
                }
            }
        }
        #endregion

        #region IReadOnlyList

        public int Count => _data.Count;

        public Message<object?> this[int index] => _data[index];

        public IEnumerator<Message<object?>> GetEnumerator() => _data.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => _data.GetEnumerator();

        #endregion
    }
}
