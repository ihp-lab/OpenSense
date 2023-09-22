using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace OpenSense.Components.BehaviorManagement {
    internal sealed class SelectorRule : CompositeRule {

        public SelectorRule(IReadOnlyList<IBehaviorRule> children) : base(children) {
        }

        #region IBehaviorRule
        public override async ValueTask<BehaviorRuleResponse> EvaluateAsync(BehaviorRequest request, CancellationToken cancellationToken = default) {
            if (_children.Count > 0) {
                foreach (var child in _children) {
                    var response = await child.EvaluateAsync(request, cancellationToken);
                    if (response.IsTriggered) {
                        return response;
                    }
                } 
            }
            return BehaviorRuleResponse.NotTriggered;
        }
        #endregion
    }
}
