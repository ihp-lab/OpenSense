using System.Composition;
using System.Windows;
using OpenSense.Components.SerialPorts;

namespace OpenSense.WPF.Components.SerialPorts {
    [Export(typeof(IInstanceControlCreator))]
    public class SerialPortPinPullerInstanceControlCreator : IInstanceControlCreator {

        public bool CanCreate(object instance) => instance is SerialPortPinPuller;

        public UIElement Create(object instance) => new SerialPortPinPullerInstanceControl() { DataContext = instance, };
    }
}
