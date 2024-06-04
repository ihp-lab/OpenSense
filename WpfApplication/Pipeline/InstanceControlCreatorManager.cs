using System.Composition;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using OpenSense.WPF.Components;

namespace OpenSense.WPF.Pipeline {
    internal sealed class InstanceControlCreatorManager {

        [ImportMany]
        private IInstanceControlCreator[] ImportedCreators { get; set; }

        public InstanceControlCreatorManager() {
            PluginBundleManager.Default.SatisfyImports(this);
            Debug.Assert(ImportedCreators is not null);
        }

        public UIElement Create(object instance) {
            var creator = ImportedCreators.FirstOrDefault(c => c.CanCreate(instance));
            return creator?.Create(instance);
        }
    }
}
