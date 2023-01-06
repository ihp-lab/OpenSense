using System.Collections;
using System.Diagnostics;

namespace OpenSense.Components.Contract.PortDataTypeInferences {
    internal sealed class InferenceStackItem {

        private readonly InferenceOperation _operation;

        public InferenceOperation Operation => _operation;

        private IEnumerator progress;

        public IEnumerator Progress {
            get => progress;
            set {
                Debug.Assert(progress is null);
                Debug.Assert(value is not null);
                progress = value;
            }
        }

        public InferenceStackItem(InferenceOperation operation) {
            _operation = operation;
        }
    }
}
