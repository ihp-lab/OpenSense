using System.Composition;
using System.Windows;
using OpenSense.Components;
using OpenSense.Components.Psi.Data;
using OpenSense.WPF.Components.Psi.Data;

namespace OpenSense.WPF.Components.Psi {
    [Export(typeof(IConfigurationControlCreator))]
    public class JsonStoreExporterConfigurationControlCreator : IConfigurationControlCreator {
        public bool CanCreate(ComponentConfiguration configuration) => configuration is JsonStoreExporterConfiguration;

        public UIElement Create(ComponentConfiguration configuration) => new JsonStoreExporterConfigurationControl() { 
            DataContext = configuration 
        };
    }
}
