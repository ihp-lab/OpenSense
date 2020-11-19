using System.Composition;
using System.Windows;
using OpenSense.Component.Contract;
using OpenSense.Component.Psi.AzureKinect;
using OpenSense.Wpf.Component.Contract;

namespace OpenSense.Wpf.Component.Psi.AzureKinect {
    [Export(typeof(IConfigurationControlCreator))]
    public class AzureKinectSensorConfigurationControlCreator : IConfigurationControlCreator {

        public bool CanCreate(ComponentConfiguration configuration) => configuration is AzureKinectSensorConfiguration;

        public UIElement Create(ComponentConfiguration configuration) => new AzureKinectSensorConfigurationControl() { DataContext = configuration };
    }
}
