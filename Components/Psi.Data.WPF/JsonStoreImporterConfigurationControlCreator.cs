using System.Composition;
using System.Windows;
using OpenSense.Components;
using OpenSense.Components.Psi.Data;

namespace OpenSense.WPF.Components.Psi.Data {
    [Export(typeof(IConfigurationControlCreator))]
    public class JsonStoreImporterConfigurationControlCreator : IConfigurationControlCreator {

        public bool CanCreate(ComponentConfiguration configuration) => configuration is JsonStoreImporterConfiguration;

        public UIElement Create(ComponentConfiguration configuration) => new JsonStoreImporterConfigurationControl() { 
            DataContext = configuration 
        };
    }
}
