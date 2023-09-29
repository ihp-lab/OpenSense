using System.Threading;
using System.Threading.Tasks;

namespace OpenSense.Components.BehaviorManagement {

    public interface IBehaviorRule {

        ValueTask<BehaviorRuleResponse> EvaluateAsync(BehaviorRequest request, CancellationToken cancellationToken = default);
    }
}
