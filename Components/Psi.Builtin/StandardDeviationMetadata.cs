﻿using System.Collections.Generic;
using System.Composition;
using System.Diagnostics;
using System.Linq;
using Microsoft.Psi;

namespace OpenSense.Components.Psi {
    [Export(typeof(IComponentMetadata))]
    public sealed class StandardDeviationMetadata : IComponentMetadata {
        public string Name => "Standard Deviation Operator";

        public string Description => "Calculate standard deviation of values of arrays.";

        public IReadOnlyList<IPortMetadata> Ports => new[] {
            new StandardDeviationPortMetadata("In", PortDirection.Input, "Input."),
            new StandardDeviationPortMetadata("Out", PortDirection.Output, "Output."),
        };

        public ComponentConfiguration CreateConfiguration() => new StandardDeviationConfiguration();

        public IProducer<T> GetProducer<T>(object instance, PortConfiguration portConfiguration) {
            Debug.Assert(Equals(this.OutputPorts().Single().Identifier, portConfiguration.Identifier));
            Debug.Assert(instance != null && HelperExtensions.CanProducerResultBeCastTo<T>(instance));
            var result = HelperExtensions.CastProducerResult<T>(instance);
            return result;
        }
    }
}
