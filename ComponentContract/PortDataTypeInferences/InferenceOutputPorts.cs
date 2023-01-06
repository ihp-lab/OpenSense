using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace OpenSense.Components.Contract.PortDataTypeInferences {
    internal sealed class InferenceOutputPorts : InferencePorts {

        public InferenceOutputPorts(
            ComponentConfiguration config,
            IReadOnlyList<ComponentConfiguration> configs,
            IImmutableList<InferenceExclusionItem> exclusions
        ) : base(config, configs, exclusions) {

        }

        #region InferenceOperation
        public override IEnumerator Run(Action<InferenceOperation> pushStackCallback) {
            throw new NotImplementedException();
        }
        #endregion
    }
}
