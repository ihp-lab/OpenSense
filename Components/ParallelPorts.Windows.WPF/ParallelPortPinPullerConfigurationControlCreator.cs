using System.Composition;
using System.Windows;
using OpenSense.Components;
using OpenSense.Components.ParallelPorts;
using OpenSense.WPF.Components.ParallelPorts;

namespace OpenSense.WPF.Components.SerialPorts {
    [Export(typeof(IConfigurationControlCreator))]
    public sealed class ParallelPortPinPullerConfigurationControlCreator : IConfigurationControlCreator {

        public bool CanCreate(ComponentConfiguration configuration) => configuration is ParallelPortPinPullerConfiguration;

        public UIElement Create(ComponentConfiguration configuration) => new ParallelPortPinPullerConfigurationControl() {
            DataContext = configuration,
        };
    }
}
