using System.Composition;
using System.Windows;
using OpenSense.Component.Contract;
using OpenSense.Component.Psi.Data;
using OpenSense.Wpf.Component.Contract;
using OpenSense.Wpf.Component.Psi.Data;

namespace OpenSense.Wpf.Component.Psi {
    [Export(typeof(IConfigurationControlCreator))]
    public class JsonStoreExporterConfigurationControlCreator : IConfigurationControlCreator {
        public bool CanCreate(ComponentConfiguration configuration) => configuration is JsonStoreExporterConfiguration;

        public UIElement Create(ComponentConfiguration configuration) => new JsonStoreExporterConfigurationControl() { DataContext = configuration };
    }
}
