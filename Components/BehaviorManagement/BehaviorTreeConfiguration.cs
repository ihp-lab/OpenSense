using System;
using Microsoft.Psi;

namespace OpenSense.Components.BehaviorManagement {
    [Serializable]
    public class BehaviorTreeConfiguration : ConventionalComponentConfiguration {

        public override IComponentMetadata GetMetadata() => new BehaviorTreeMetadata();

        protected override object Instantiate(Pipeline pipeline, IServiceProvider serviceProvider) {
            var root = new MimicRule<bool>();
            var result = new BehaviorTree(pipeline, root, DeliveryPolicy.Unlimited);
            return result;
        }
    }
}
