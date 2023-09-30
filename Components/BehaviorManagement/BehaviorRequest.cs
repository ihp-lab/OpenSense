using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace OpenSense.Components.BehaviorManagement {

    public readonly struct BehaviorRequest : IReadOnlyDictionary<IPortMetadata, BehaviorInputData> {

        private readonly IReadOnlyDictionary<IPortMetadata, BehaviorInputData> _inputs;

        public DateTime OriginatingTime { get; }

        public TimeSpan Window { get; }

        public BehaviorRequest(DateTime originatingTime, TimeSpan window, IReadOnlyDictionary<IPortMetadata, BehaviorInputData> inputs) {
            _inputs = inputs;
            OriginatingTime = originatingTime;
            Window = window;
            Debug.Assert(_inputs.All(kv => ReferenceEquals(kv.Key, kv.Value.Port)));
        }

        #region IReadOnlyDictionary
        public BehaviorInputData this[IPortMetadata key] => _inputs[key];

        public IEnumerable<IPortMetadata> Keys => _inputs.Keys;

        public IEnumerable<BehaviorInputData> Values => _inputs.Values;

        public int Count => _inputs.Count;

        public bool ContainsKey(IPortMetadata key) => _inputs.ContainsKey(key);
        public IEnumerator<KeyValuePair<IPortMetadata, BehaviorInputData>> GetEnumerator() => _inputs.GetEnumerator();
        public bool TryGetValue(IPortMetadata key, out BehaviorInputData value) => _inputs.TryGetValue(key, out value);
        IEnumerator IEnumerable.GetEnumerator() => _inputs.GetEnumerator();
        #endregion
    }
}
