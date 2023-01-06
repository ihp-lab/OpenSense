using System.Composition;
using System.Windows;
using OpenSense.Component.Contract;
using OpenSense.Component.Psi.Data;
using OpenSense.WPF.Component.Contract;
using OpenSense.WPF.Component.Psi.Data;

namespace OpenSense.WPF.Component.Psi {
    [Export(typeof(IConfigurationControlCreator))]
    public class JsonStoreExporterConfigurationControlCreator : IConfigurationControlCreator {
        public bool CanCreate(ComponentConfiguration configuration) => configuration is JsonStoreExporterConfiguration;

        public UIElement Create(ComponentConfiguration configuration) => new JsonStoreExporterConfigurationControl() { DataContext = configuration };
    }
}
