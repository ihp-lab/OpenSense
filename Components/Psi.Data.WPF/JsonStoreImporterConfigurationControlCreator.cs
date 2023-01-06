using System.Composition;
using System.Windows;
using OpenSense.Components.Contract;
using OpenSense.Components.Psi.Data;
using OpenSense.WPF.Components.Contract;

namespace OpenSense.WPF.Components.Psi.Data {
    [Export(typeof(IConfigurationControlCreator))]
    public class JsonStoreImporterConfigurationControlCreator : IConfigurationControlCreator {

        public bool CanCreate(ComponentConfiguration configuration) => configuration is JsonStoreImporterConfiguration;

        public UIElement Create(ComponentConfiguration configuration) => new JsonStoreImporterConfigurationControl() { DataContext = configuration };
    }
}
