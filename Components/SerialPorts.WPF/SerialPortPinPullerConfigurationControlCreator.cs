using System.Composition;
using System.Windows;
using OpenSense.Components;
using OpenSense.Components.SerialPorts;

namespace OpenSense.WPF.Components.SerialPorts {
    [Export(typeof(IConfigurationControlCreator))]
    public sealed class SerialPortPinPullerConfigurationControlCreator : IConfigurationControlCreator {

        public bool CanCreate(ComponentConfiguration configuration) => configuration is SerialPortPinPullerConfiguration;

        public UIElement Create(ComponentConfiguration configuration) => new SerialPortPinPullerConfigurationControl() {
            DataContext = configuration,
        };
    }
}
