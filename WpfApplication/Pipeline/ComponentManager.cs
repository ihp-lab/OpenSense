#nullable enable

using System.Collections.Generic;
using System.Composition;
using System.Diagnostics;
using OpenSense.Components;

namespace OpenSense.WPF.Pipeline {
    internal sealed class ComponentManager {

        [ImportMany]
        private IComponentMetadata[] ImportedComponents { get; set; } = null!;

        public IReadOnlyList<IComponentMetadata> Components => ImportedComponents;

        public ComponentManager() {
            PluginBundleManager.Default.SatisfyImports(this);
            Debug.Assert(ImportedComponents is not null);
        }
    }
}
