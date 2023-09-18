using System;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace OpenSense.Components.PortDataTypeInferences {
    internal abstract class InferencePort : InferenceOperation<Type> {

        protected readonly IPortMetadata _portMetadata;

        protected InferencePort(
            ComponentConfiguration config,
            IPortMetadata portMetadata,
            IReadOnlyList<ComponentConfiguration> configs,
            IImmutableList<InferenceExclusionItem> exclusions
        ) : base(config, configs, exclusions) {
            _portMetadata = portMetadata;
        }
    }
}
