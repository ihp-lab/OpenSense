using System;
using System.Composition;

namespace OpenSense.Components.BehaviorManagement {
    [Export(typeof(IComponentMetadata))]
    public class BehaviorTreeMetadata : ConventionalComponentMetadata {

        public override string Description => "Behavior Tree for Behavior Management.";

        protected override Type ComponentType => typeof(BehaviorTree);

        public override string Name => "Behavior Tree";

        protected override string? GetPortDescription(string portName) {
            switch (portName) {
                default:
                    return null;
            }
        }

        public override ComponentConfiguration CreateConfiguration() => new BehaviorTreeConfiguration();
    }
}
