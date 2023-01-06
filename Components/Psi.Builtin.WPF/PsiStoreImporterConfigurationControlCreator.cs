using System.Composition;
using System.Windows;
using OpenSense.Components.Contract;
using OpenSense.Components.Psi;
using OpenSense.WPF.Components.Contract;

namespace OpenSense.WPF.Components.Psi {
    [Export(typeof(IConfigurationControlCreator))]
    public class PsiStoreImporterConfigurationControlCreator : IConfigurationControlCreator {

        public bool CanCreate(ComponentConfiguration configuration) => configuration is PsiStoreImporterConfiguration;

        public UIElement Create(ComponentConfiguration configuration) => new PsiStoreImporterConfigurationControl() { DataContext = configuration };
    }
}
