using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;

namespace OpenSense.Component.Contract.PortDataTypeInferences {
    internal abstract class InferenceOperation {

        protected readonly ComponentConfiguration _config;

        protected readonly IReadOnlyList<ComponentConfiguration> _configs;

        protected readonly IImmutableList<InferenceExclusionItem> _exclusions;

        #region Constructors
        public InferenceOperation(
            ComponentConfiguration config,
            IReadOnlyList<ComponentConfiguration> configs,
            IImmutableList<InferenceExclusionItem> exclusions
        ) {
            _config = config;
            _configs = configs;
            _exclusions = exclusions;
        }
        #endregion

        #region APIs
        public abstract IEnumerator Run(Action<InferenceOperation> pushStackCallback);
        #endregion
    }

    internal abstract class InferenceOperation<T> : InferenceOperation {

        private bool hasResult;

        private T result;

        #region Constructors
        public InferenceOperation(
            ComponentConfiguration self,
            IReadOnlyList<ComponentConfiguration> configs,
            IImmutableList<InferenceExclusionItem> exclusions
        ) : base(self, configs, exclusions) {
            
        }
        #endregion

        protected void SetResult(T result) {
            Debug.Assert(!hasResult);
            this.result = result;
            hasResult = true;
        }

        public T GetResult() {
            Debug.Assert(hasResult);
            return result;
        }
    }
}
