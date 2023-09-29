using System;
using System.Collections.Generic;
using System.Composition;

namespace OpenSense.Components.BehaviorManagement {
    [Export(typeof(BehaviorRuleConfiguration))]
    [Serializable]
    public sealed class MimicRuleConfiguration : BehaviorRuleConfiguration {

        internal static readonly StaticPortMetadata InputPort = new StaticPortMetadata("In", PortDirection.Input, PortAggregation.Object, typeof(object), "The input to mimic.");

        internal static readonly StaticPortMetadata OutputPort = new StaticPortMetadata("Out", PortDirection.Output, PortAggregation.Object, typeof(object), "The mimicked output.");

        private static readonly StaticPortMetadata[] StaticPorts = new StaticPortMetadata[] { InputPort, OutputPort, };

        #region BehaviorRuleConfiguration
        public override TimeSpan Window => 
            TimeSpan.Zero;

        public override IReadOnlyCollection<IPortMetadata> Ports => 
            StaticPorts;

        public override IBehaviorRule Instantiate(IServiceProvider? serviceProvider) =>
            new MimicRule() { 
            };
        #endregion
    }
}
