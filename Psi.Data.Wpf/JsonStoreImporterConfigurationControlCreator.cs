using System.Composition;
using System.Windows;
using OpenSense.Component.Contract;
using OpenSense.Component.Psi.Data;
using OpenSense.Wpf.Component.Contract;

namespace OpenSense.Wpf.Component.Psi.Data {
    [Export(typeof(IConfigurationControlCreator))]
    public class JsonStoreImporterConfigurationControlCreator : IConfigurationControlCreator {

        public bool CanCreate(ComponentConfiguration configuration) => configuration is JsonStoreImporterConfiguration;

        public UIElement Create(ComponentConfiguration configuration) => new JsonStoreImporterConfigurationControl() { DataContext = configuration };
    }
}
