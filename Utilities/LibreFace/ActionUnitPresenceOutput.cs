using Microsoft.ML.OnnxRuntime;
using System.Collections;
using System.Collections.Immutable;
using System.Diagnostics;

namespace LibreFace {
    public sealed record class ActionUnitPresenceOutput : IReadOnlyDictionary<string, bool> {

        private const float Threshold = 0.5f;

        private const int Labels = 12;

        public static readonly string[] Keys = {
            "1", "2", "4", "6", "7", "10", "12", "14", "15", "17", "23", "24",
        };

        private static readonly KeyComparer Comparer = new KeyComparer();

        public static readonly ActionUnitPresenceOutput Empty = new ActionUnitPresenceOutput();

        private readonly RawValuesDictionary _values;

        public IReadOnlyDictionary<string, float> RawValues => _values;

        internal ActionUnitPresenceOutput(in NamedOnnxValue labels) {
            var dict = labels.AsEnumerable<float>()
                .Select((v, i) => (Value: v, Idx: i))
                .ToImmutableSortedDictionary(t => Keys[t.Idx], t => t.Value, Comparer);
            _values = new RawValuesDictionary(dict);
            Debug.Assert(_values.Count == Labels);
        }

        private ActionUnitPresenceOutput() {
            _values = new RawValuesDictionary(ImmutableSortedDictionary<string, float>.Empty);
        }

        private static bool ToBool(float value) => value >= Threshold;

        #region IReadOnlyCollection
        public bool this[string key] => ToBool(_values[key]);

        public int Count => _values.Count;

        IEnumerable<string> IReadOnlyDictionary<string, bool>.Keys => _values.Keys;

        public IEnumerable<bool> Values => _values.Values.Select(ToBool);

        public IEnumerator<KeyValuePair<string, bool>> GetEnumerator() => 
            _values.Select(kv => new KeyValuePair<string, bool>(kv.Key, ToBool(kv.Value))).GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public bool ContainsKey(string key) => _values.ContainsKey(key);

        public bool TryGetValue(string key, out bool value) {
            var result = _values.TryGetValue(key, out var temp);
            value = result ? ToBool(temp) : default;
            return result;
        }
        #endregion

        #region Classes
        private sealed class KeyComparer : IComparer<string> {
            public int Compare(string x, string y) {
                var i = Array.IndexOf(Keys, x);
                var j = Array.IndexOf(Keys, y);
                var result = i.CompareTo(j);
                return result;
            }
        }

        private readonly struct RawValuesDictionary : IReadOnlyDictionary<string, float> {

            private readonly ImmutableSortedDictionary<string, float> _values;

            public RawValuesDictionary(ImmutableSortedDictionary<string, float> values) {
                _values = values;
            }

            #region IReadOnlyDictionary
            public float this[string key] => _values[key];

            public IEnumerable<string> Keys => _values.Keys;

            public IEnumerable<float> Values => _values.Values;

            public int Count => _values.Count;

            public bool ContainsKey(string key) => _values.ContainsKey(key);

            public IEnumerator<KeyValuePair<string, float>> GetEnumerator() => _values.GetEnumerator();

            public bool TryGetValue(string key, out float value) => _values.TryGetValue(key, out value);

            IEnumerator IEnumerable.GetEnumerator() => _values.GetEnumerator();
            #endregion
        }
        #endregion
    }
}
