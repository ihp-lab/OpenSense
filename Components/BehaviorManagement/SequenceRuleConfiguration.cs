using System;
using System.Collections.Generic;
using System.Composition;
using System.Linq;

namespace OpenSense.Components.BehaviorManagement {
    [Export(typeof(BehaviorRuleConfiguration))]
    [Serializable]
    internal sealed class SequenceRuleConfiguration : CompositeRuleConfiguration {

        private readonly List<BehaviorRuleConfiguration> _children = new List<BehaviorRuleConfiguration>();

        #region CompositeRuleConfiguration
        public override BehaviorRuleConfiguration this[int index] => 
            _children[index];

        public override int Count => 
            _children.Count;

        public override IEnumerator<BehaviorRuleConfiguration> GetEnumerator() => 
            _children.GetEnumerator();

        public override IBehaviorRule Instantiate(IServiceProvider? serviceProvider) {
            var children = _children
                .Select(c => c.Instantiate(serviceProvider))
                .ToArray();
            var result = new SequenceRule(children);
            return result;
        }
        #endregion
    }
}
