using System.Composition;
using System.Windows;
using OpenSense.Components;
using OpenSense.Components.Psi;
using OpenSense.WPF.Components;

namespace OpenSense.WPF.Components.Psi {
    [Export(typeof(IConfigurationControlCreator))]
    public class RemoteImporterConfigurationControlCreator : IConfigurationControlCreator {
        public bool CanCreate(ComponentConfiguration configuration) => configuration is RemoteImporterConfiguration;

        public UIElement Create(ComponentConfiguration configuration) => new RemoteImporterConfigurationControl() { DataContext = configuration };
    }
}
