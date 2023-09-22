using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace OpenSense.Components.BehaviorManagement {
    public readonly struct BehaviorRuleResponse {

        private static readonly IReadOnlyCollection<BehaviorOutputData> EmptyOutputs = Array.Empty<BehaviorOutputData>();

        public static readonly BehaviorRuleResponse NotTriggered = new BehaviorRuleResponse(false, EmptyOutputs);

        public static readonly ValueTask<BehaviorRuleResponse> NotTriggeredTask = new ValueTask<BehaviorRuleResponse>(NotTriggered);

        public bool IsTriggered { get; }

        public IReadOnlyCollection<BehaviorOutputData> Outputs { get; }

        public BehaviorRuleResponse(bool isTriggered, IReadOnlyCollection<BehaviorOutputData> outputs) {
            IsTriggered = isTriggered;
            Outputs = outputs;
        }

        #region Helpers
        public static BehaviorRuleResponse FromResults(params BehaviorOutputData[] outputs) {
            var result = new BehaviorRuleResponse(true, outputs);
            return result;
        }

        public static BehaviorRuleResponse FromResults(IReadOnlyCollection<BehaviorOutputData> outputs) {
            var result = new BehaviorRuleResponse(true, outputs);
            return result;
        }

        public ValueTask<BehaviorRuleResponse> AsValueTask() {
            var result = new ValueTask<BehaviorRuleResponse>(this);
            return result;  
        }
        #endregion
    }
}
