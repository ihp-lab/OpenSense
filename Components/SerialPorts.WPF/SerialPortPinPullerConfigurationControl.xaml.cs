#nullable enable

using System.Windows.Controls;
using OpenSense.Components.SerialPorts;

namespace OpenSense.WPF.Components.SerialPorts {
    public sealed partial class SerialPortPinPullerConfigurationControl : UserControl {

        private SerialPortPinPullerConfiguration? Configuration => (SerialPortPinPullerConfiguration)DataContext;

        public SerialPortPinPullerConfigurationControl() {
            InitializeComponent();
        }
    }
}
