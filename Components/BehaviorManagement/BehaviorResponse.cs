using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace OpenSense.Components.BehaviorManagement {
    public readonly struct BehaviorResponse : IReadOnlyDictionary<IPortMetadata, BehaviorOutputData> {

        //TODO: Type load failure

        private static IReadOnlyDictionary<IPortMetadata, BehaviorOutputData> EmptyOutputs => new Dictionary<IPortMetadata, BehaviorOutputData>(0);//private static readonly IReadOnlyCollection<BehaviorOutputData> EmptyOutputs = Array.Empty<BehaviorOutputData>();

        public static BehaviorResponse NotTriggered => new BehaviorResponse(false, EmptyOutputs);//public static readonly BehaviorResponse NotTriggered = new BehaviorResponse(false, EmptyOutputs);

        public static ValueTask<BehaviorResponse> NotTriggeredTask => new ValueTask<BehaviorResponse>(NotTriggered);//public static readonly ValueTask<BehaviorResponse> NotTriggeredTask = new ValueTask<BehaviorResponse>(NotTriggered);

        private readonly IReadOnlyDictionary<IPortMetadata, BehaviorOutputData> _outputs;

        public bool IsTriggered { get; }

        internal BehaviorResponse(bool isTriggered, IReadOnlyDictionary<IPortMetadata, BehaviorOutputData> data) {
            _outputs = data;
            IsTriggered = isTriggered;
        }

        internal BehaviorResponse(bool isTriggered, IReadOnlyList<BehaviorOutputData> data) {
            IsTriggered = isTriggered;
            var dict = new Dictionary<IPortMetadata, BehaviorOutputData>(data.Count);
            foreach (var item in data) {
                dict.Add(item.Port, item);
            }
            _outputs = dict;
        }

        #region Helpers

        public static BehaviorResponse FromResults(params BehaviorOutputData[] outputs) {
            var result = new BehaviorResponse(true, outputs);
            return result;
        }

        public static BehaviorResponse FromResults(IReadOnlyList<BehaviorOutputData> outputs) {
            var result = new BehaviorResponse(true, outputs);
            return result;
        }

        public ValueTask<BehaviorResponse> AsValueTask() {
            var result = new ValueTask<BehaviorResponse>(this);
            return result;  
        }
        #endregion

        #region IReadOnlyDictionary
        public BehaviorOutputData this[IPortMetadata key] => _outputs[key];

        public IEnumerable<IPortMetadata> Keys => _outputs.Keys;

        public IEnumerable<BehaviorOutputData> Values => _outputs.Values;

        public int Count => _outputs.Count;

        public bool ContainsKey(IPortMetadata key) => _outputs.ContainsKey(key);

        public IEnumerator<KeyValuePair<IPortMetadata, BehaviorOutputData>> GetEnumerator() => _outputs.GetEnumerator();

        public bool TryGetValue(IPortMetadata key, out BehaviorOutputData value) => _outputs.TryGetValue(key, out value);

        IEnumerator IEnumerable.GetEnumerator() => _outputs.GetEnumerator();
        #endregion
    }
}
