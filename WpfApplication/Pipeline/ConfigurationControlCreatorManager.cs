using System.Composition;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using OpenSense.Components;
using OpenSense.WPF.Components;

namespace OpenSense.WPF.Pipeline {
    internal sealed class ConfigurationControlCreatorManager {

        [ImportMany]
        private IConfigurationControlCreator[] ImportedCreators { get; set; }

        public ConfigurationControlCreatorManager() {
            PluginBundleManager.Default.SatisfyImports(this);
            Debug.Assert(ImportedCreators is not null);
        }

        public UIElement Create(ComponentConfiguration configuration) {
            var creator = ImportedCreators.FirstOrDefault(c => c.CanCreate(configuration));
            return creator?.Create(configuration);
        }
    }
}
