using Microsoft.ML.OnnxRuntime;
using System.Collections;
using System.Collections.Immutable;
using System.Diagnostics;

namespace LibreFace {
    public sealed class ExpressionOutput : IReadOnlyDictionary<string, float> {

        private const int Labels = 8;

        public static readonly string[] Keys = {
            "Neutral", "Happiness", "Sadness", "Surprise", "Fear", "Disgust", "Anger", "Contempt",
        };

        private static readonly KeyComparer Comparer = new KeyComparer();

        private readonly IReadOnlyDictionary<string, float> _dict;

        internal ExpressionOutput(in NamedOnnxValue labels) {
            _dict = labels.AsEnumerable<float>()
                //.Select(l => MathF.Max(0, MathF.Min(5, l * 5f))) //Apply for ResNet18 model
                .Select((l, i) => (Label: l, Idx: i))
                .ToImmutableSortedDictionary(t => Keys[t.Idx], t => t.Label, Comparer);
            Debug.Assert(_dict.Count == Labels);
        }

        #region IReadOnlyCollection
        public float this[string value] => _dict[value];

        public int Count => _dict.Count;

        IEnumerable<string> IReadOnlyDictionary<string, float>.Keys => _dict.Keys;

        public IEnumerable<float> Values => _dict.Values;

        public IEnumerator<KeyValuePair<string, float>> GetEnumerator() => _dict.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable)_dict).GetEnumerator();

        public bool ContainsKey(string key) => _dict.ContainsKey(key);

        public bool TryGetValue(string key, out float value) => _dict.TryGetValue(key, out value);
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
