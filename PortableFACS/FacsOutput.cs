using Microsoft.ML.OnnxRuntime;
using System.Collections;
using System.Diagnostics;

namespace PortableFACS {
    public sealed record class FacsOutput : IReadOnlyDictionary<int, float> {

        private const int Labels = 12;

        public static readonly int[] Keys = {
            1, 2, 4, 5, 6, 9, 12, 15, 17, 20, 25, 26,
        };

        private readonly IReadOnlyDictionary<int, float> _dict;

        internal FacsOutput(in NamedOnnxValue labels) {
            _dict = labels.AsEnumerable<float>()
                .Select(l => MathF.Max(0, MathF.Min(5, l * 5f))) //Apply for ResNet18 model
                .Select((l, i) => (Label: l, Idx: i))
                .ToDictionary(t => Keys[t.Idx], t => t.Label);
            Debug.Assert(_dict.Count == Labels);
        }

        #region IReadOnlyCollection
        public float this[int value] => _dict[value];

        public int Count => _dict.Count;

        IEnumerable<int> IReadOnlyDictionary<int, float>.Keys => _dict.Keys;

        public IEnumerable<float> Values => _dict.Values;

        public IEnumerator<KeyValuePair<int, float>> GetEnumerator() => _dict.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable)_dict).GetEnumerator();

        public bool ContainsKey(int key) => _dict.ContainsKey(key);

        public bool TryGetValue(int key, out float value) => _dict.TryGetValue(key, out value);
        #endregion
    }
}
