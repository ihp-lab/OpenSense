using System;
using System.Collections.Generic;
using CommunityToolkit.Mvvm.ComponentModel;

namespace OpenSense.Components.BehaviorManagement {
    [Serializable]
    public abstract class BehaviorRuleConfiguration : ObservableObject {

        public abstract TimeSpan Window { get; }

        public abstract IReadOnlyCollection<IPortMetadata> Ports { get; }

        public abstract IBehaviorRule Instantiate(IServiceProvider? serviceProvider);
    }
}
