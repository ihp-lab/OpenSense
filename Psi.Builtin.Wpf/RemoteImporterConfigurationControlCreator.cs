using System.Composition;
using System.Windows;
using OpenSense.Component.Contract;
using OpenSense.Component.Psi;
using OpenSense.WPF.Component.Contract;

namespace OpenSense.WPF.Component.Psi {
    [Export(typeof(IConfigurationControlCreator))]
    public class RemoteImporterConfigurationControlCreator : IConfigurationControlCreator {
        public bool CanCreate(ComponentConfiguration configuration) => configuration is RemoteImporterConfiguration;

        public UIElement Create(ComponentConfiguration configuration) => new RemoteImporterConfigurationControl() { DataContext = configuration };
    }
}
