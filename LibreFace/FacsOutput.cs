using Microsoft.ML.OnnxRuntime;
using System.Collections;
using System.Diagnostics;

namespace LibreFace {
    public sealed record class FacsOutput : IReadOnlyDictionary<string, float> {

        private const int Labels = 12;

        public static readonly string[] Keys = {
            "AU01", "AU02", "AU04", "AU05", "AU06", "AU09", "AU12", "AU15", "AU17", "AU20", "AU25", "AU26",
        };

        private readonly IReadOnlyDictionary<string, float> _dict;

        internal FacsOutput(in NamedOnnxValue labels) {
            _dict = labels.AsEnumerable<float>()
                .Select(l => MathF.Max(0, MathF.Min(5, l * 5f))) //Apply for ResNet18 model
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
