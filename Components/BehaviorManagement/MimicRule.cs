using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace OpenSense.Components.BehaviorManagement {
    internal sealed class MimicRule<T> : IBehaviorRule {

        private static readonly StaticPortMetadata InputPort = new StaticPortMetadata("In", PortDirection.Input, PortAggregation.Object, typeof(T), "The input to mimic.");
        private static readonly StaticPortMetadata OutputPort = new StaticPortMetadata("Out", PortDirection.Output, PortAggregation.Object, typeof(T), "The mimicked output.");
        private static readonly StaticPortMetadata[] StaticPorts = new StaticPortMetadata[] { InputPort, OutputPort, };

        #region IBehaviorRule
        public TimeSpan Window => TimeSpan.Zero;

        public IReadOnlyCollection<IPortMetadata> Ports => StaticPorts;

        public ValueTask<BehaviorRuleResponse> EvaluateAsync(BehaviorRequest request, CancellationToken cancellationToken = default) {
            if (request[InputPort] is not { } input) {
                return BehaviorRuleResponse.NotTriggeredTask;
            }
            var last = input.Last();
            var output = new BehaviorOutputData(OutputPort, input.DataType, last.Data, last.Envelope);
            var response = BehaviorRuleResponse.FromResults(output);
            return response.AsValueTask();
        }
        #endregion
    }
}
