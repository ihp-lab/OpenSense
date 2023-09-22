using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace OpenSense.Components.BehaviorManagement {
    internal sealed class SequenceRule : CompositeRule {

        public SequenceRule(IReadOnlyList<IBehaviorRule> children) : base(children) {
        }

        #region IBehaviorRule
        public override async ValueTask<BehaviorRuleResponse> EvaluateAsync(BehaviorRequest request, CancellationToken cancellationToken = default) {
            if (_children.Count > 0) {
                var results = new List<BehaviorOutputData>();
                foreach (var child in _children) {
                    var response = await child.EvaluateAsync(request, cancellationToken);
                    if (!response.IsTriggered) {
                        return BehaviorRuleResponse.NotTriggered;
                    }
                    results.AddRange(response.Outputs);
                }
                return BehaviorRuleResponse.FromResults(results);
            }
            return BehaviorRuleResponse.NotTriggered;
        }
        #endregion
    }
}
