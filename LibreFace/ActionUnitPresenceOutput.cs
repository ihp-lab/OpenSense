using Microsoft.ML.OnnxRuntime;
using System.Collections;
using System.Collections.Immutable;
using System.Diagnostics;

namespace LibreFace {
    public sealed record class ActionUnitPresenceOutput : IReadOnlyDictionary<string, bool> {

        private const int Labels = 12;

        public static readonly string[] Keys = {
            "1", "2", "4", "6", "7", "10", "12", "14", "15", "17", "23", "24",
        };

        private static readonly KeyComparer Comparer = new KeyComparer();

        private readonly IReadOnlyDictionary<string, bool> _dict;

        internal ActionUnitPresenceOutput(in NamedOnnxValue labels) {
            _dict = labels.AsEnumerable<float>()
                .Select(l => l >= 0.5)
                .Select((l, i) => (Label: l, Idx: i))
                .ToImmutableSortedDictionary(t => Keys[t.Idx], t => t.Label, Comparer);
            Debug.Assert(_dict.Count == Labels);
        }

        #region IReadOnlyCollection
        public bool this[string value] => _dict[value];

        public int Count => _dict.Count;

        IEnumerable<string> IReadOnlyDictionary<string, bool>.Keys => _dict.Keys;

        public IEnumerable<bool> Values => _dict.Values;

        public IEnumerator<KeyValuePair<string, bool>> GetEnumerator() => _dict.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable)_dict).GetEnumerator();

        public bool ContainsKey(string key) => _dict.ContainsKey(key);

        public bool TryGetValue(string key, out bool value) => _dict.TryGetValue(key, out value);
        #endregion

        #region Comparer
        private sealed class KeyComparer : IComparer<string> {
            public int Compare(string x, string y) {
                var i = Array.IndexOf(Keys, x);
                var j = Array.IndexOf(Keys, y);
                var result = i.CompareTo(j);
                return result;
            }
        }
        #endregion
    }
}
