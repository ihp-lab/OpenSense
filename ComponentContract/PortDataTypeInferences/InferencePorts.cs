using System;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace OpenSense.Components.PortDataTypeInferences {
    internal abstract class InferencePorts : InferenceOperation<IList<Type>> {

        protected InferencePorts(
            ComponentConfiguration config,
            IReadOnlyList<ComponentConfiguration> configs,
            IImmutableList<InferenceExclusionItem> exclusions
        ) : base(config, configs, exclusions) {

        }
    }
}
