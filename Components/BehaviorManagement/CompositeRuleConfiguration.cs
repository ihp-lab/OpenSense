using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace OpenSense.Components.BehaviorManagement {
    [Serializable]
    public abstract class CompositeRuleConfiguration : BehaviorRuleConfiguration, IReadOnlyList<BehaviorRuleConfiguration> {

        #region BehaviorRuleConfiguration
        public override TimeSpan Window => this
            .Select(r => r.Window)
            .Aggregate(TimeSpan.Zero, (a, v) => a > v ? a : v)
            ;

        public override IReadOnlyCollection<IPortMetadata> Ports => this
            .SelectMany(c => c.Ports)
            .ToArray()
            ;
        #endregion

        #region IReadOnlyList
        public abstract BehaviorRuleConfiguration this[int index] { get; }

        public abstract int Count { get; }

        public abstract IEnumerator<BehaviorRuleConfiguration> GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
        #endregion
    }
}
