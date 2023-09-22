using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace OpenSense.Components.BehaviorManagement {

    public interface IBehaviorRule {

        TimeSpan Window { get; }

        IReadOnlyCollection<IPortMetadata> Ports { get; }

        ValueTask<BehaviorRuleResponse> EvaluateAsync(BehaviorRequest request, CancellationToken cancellationToken = default);
    }
}
