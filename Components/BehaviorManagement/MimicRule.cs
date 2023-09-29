using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace OpenSense.Components.BehaviorManagement {
    public sealed class MimicRule : IBehaviorRule {

        #region IBehaviorRule
        public ValueTask<BehaviorRuleResponse> EvaluateAsync(BehaviorRequest request, CancellationToken cancellationToken = default) {
            if (request[MimicRuleConfiguration.InputPort] is not { } input) {
                return BehaviorRuleResponse.NotTriggeredTask;
            }
            var last = input.Last();
            var output = new BehaviorOutputData(MimicRuleConfiguration.OutputPort, input.DataType, last.Data, last.Envelope);
            var response = BehaviorRuleResponse.FromResults(output);
            return response.AsValueTask();
        }
        #endregion
    }
}
