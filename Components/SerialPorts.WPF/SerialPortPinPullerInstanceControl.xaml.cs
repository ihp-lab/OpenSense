#nullable enable

using System.Windows.Controls;
using OpenSense.Components.SerialPorts;

namespace OpenSense.WPF.Components.SerialPorts {
    public partial class SerialPortPinPullerInstanceControl : UserControl {

        private SerialPortPinPuller? Instance => (SerialPortPinPuller)DataContext;

        public SerialPortPinPullerInstanceControl() {
            InitializeComponent();
        }
    }
}
