using System.Collections.Generic;
using System.Composition;
using System.Diagnostics;

namespace OpenSense.WPF.Widgets {
    public class WidgetManager {

        [ImportMany]
        private IWidgetMetadata[] ImportedWeidgets { get; set; }

        public IReadOnlyList<IWidgetMetadata> Widgets => ImportedWeidgets;

        public WidgetManager() {
            PluginBundleManager.Default.SatisfyImports(this);
            Debug.Assert(ImportedWeidgets is not null);
        }
    }
}
