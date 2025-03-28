using System.Composition;
using OpenSense.Components;
using System.Windows;
using OpenSense.Components.AzureKinect.Sensor;

namespace OpenSense.WPF.Components.AzureKinect.Sensor {
    [Export(typeof(IConfigurationControlCreator))]
    public sealed class AzureKinectSensorConfigurationControlCreator : IConfigurationControlCreator {

        public bool CanCreate(ComponentConfiguration configuration) => configuration is AzureKinectSensorConfiguration;

        public UIElement Create(ComponentConfiguration configuration) => new AzureKinectSensorConfigurationControl() {
            DataContext = configuration
        };
    }
}
