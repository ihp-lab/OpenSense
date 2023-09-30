using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace OpenSense.Components.BehaviorManagement {
    public sealed class MimicRule : IBehaviorRule {

        #region IBehaviorRule
        public ValueTask<BehaviorResponse> EvaluateAsync(BehaviorRequest request, CancellationToken cancellationToken = default) {
            if (!request.TryGetValue(MimicRuleConfiguration.InputPort, out var input)) {
                return BehaviorResponse.NotTriggeredTask;
            }
            var last = input.Last();
            var output = new BehaviorOutputData(MimicRuleConfiguration.OutputPort, input.DataType, last.Data, request.OriginatingTime);
            var response = BehaviorResponse.FromResults(output);
            return response.AsValueTask();
        }
        #endregion
    }
}
