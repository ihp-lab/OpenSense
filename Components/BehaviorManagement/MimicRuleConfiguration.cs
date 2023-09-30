using System;
using System.Collections.Generic;
using System.Composition;

namespace OpenSense.Components.BehaviorManagement {
    [Export(typeof(BehaviorRuleConfiguration))]
    [Serializable]
    public sealed class MimicRuleConfiguration : BehaviorRuleConfiguration {

        internal static readonly MirrorPortMetadata InputPort = new MirrorPortMetadata("In", PortDirection.Input, "The input to mimic.");

        internal static readonly MirrorPortMetadata OutputPort = new MirrorPortMetadata("Out", PortDirection.Output, "The mimicked output.");

        private static readonly MirrorPortMetadata[] StaticPorts = new MirrorPortMetadata[] { InputPort, OutputPort, };

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
