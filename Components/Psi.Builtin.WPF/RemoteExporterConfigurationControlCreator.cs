using System.Composition;
using System.Windows;
using OpenSense.Components;
using OpenSense.Components.Psi;

namespace OpenSense.WPF.Components.Psi {
    [Export(typeof(IConfigurationControlCreator))]
    public class RemoteExporterConfigurationControlCreator : IConfigurationControlCreator {

        public bool CanCreate(ComponentConfiguration configuration) => configuration is RemoteExporterConfiguration;

        public UIElement Create(ComponentConfiguration configuration) => new RemoteExporterConfigurationControl((RemoteExporterConfiguration)configuration);
    }
}
