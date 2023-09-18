using System.Composition;
using System.Windows;
using OpenSense.Components;
using OpenSense.Components.Psi;
using OpenSense.WPF.Components;

namespace OpenSense.WPF.Components.Psi {
    [Export(typeof(IConfigurationControlCreator))]
    public class PsiStoreExporterConfigurationControlCreator : IConfigurationControlCreator {
        public bool CanCreate(ComponentConfiguration configuration) => configuration is PsiStoreExporterConfiguration;

        public UIElement Create(ComponentConfiguration configuration) => new PsiStoreExporterConfigurationControl((PsiStoreExporterConfiguration)configuration);
    }
}
