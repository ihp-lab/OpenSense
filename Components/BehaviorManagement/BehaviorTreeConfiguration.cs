using System;
using System.Collections.Generic;
using Microsoft.Psi;

namespace OpenSense.Components.BehaviorManagement {
    [Serializable]
    public class BehaviorTreeConfiguration : ComponentConfiguration {

        private BehaviorRuleConfiguration? root;

        public BehaviorRuleConfiguration? Root {
            get => root;
            set => SetProperty(ref root, value);
        }

        #region ComponentConfiguration
        public override IComponentMetadata GetMetadata() => 
            new BehaviorTreeMetadata(this);

        public override object Instantiate(Pipeline pipeline, IReadOnlyList<ComponentEnvironment> instantiatedComponents, IServiceProvider? serviceProvider) {
            throw new NotImplementedException();
        }
        #endregion
    }
}
