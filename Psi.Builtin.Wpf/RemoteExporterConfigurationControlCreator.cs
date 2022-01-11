using System.Composition;
using System.Windows;
using OpenSense.Component.Contract;
using OpenSense.Component.Psi;
using OpenSense.Wpf.Component.Contract;

namespace OpenSense.Wpf.Component.Psi {
    [Export(typeof(IConfigurationControlCreator))]
    public class RemoteExporterConfigurationControlCreator : IConfigurationControlCreator {

        public bool CanCreate(ComponentConfiguration configuration) => configuration is RemoteExporterConfiguration;

        public UIElement Create(ComponentConfiguration configuration) => new RemoteExporterConfigurationControl((RemoteExporterConfiguration)configuration);
    }
}
