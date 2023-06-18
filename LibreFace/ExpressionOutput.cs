using Microsoft.ML.OnnxRuntime;
using System.Collections;
using System.Diagnostics;

namespace LibreFace {
    public sealed class ExpressionOutput : IReadOnlyDictionary<string, float> {

        private const int Labels = 8;

        public static readonly string[] Keys = {
            "FE0", "FE1", "FE2", "FE3", "FE4", "FE5", "FE6", "FE7",
        };

        private readonly IReadOnlyDictionary<string, float> _dict;

        internal ExpressionOutput(in NamedOnnxValue labels) {
            _dict = labels.AsEnumerable<float>()
                //.Select(l => MathF.Max(0, MathF.Min(5, l * 5f))) //Apply for ResNet18 model
                .Select((l, i) => (Label: l, Idx: i))
                .ToDictionary(t => Keys[t.Idx], t => t.Label);
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
    }
}
