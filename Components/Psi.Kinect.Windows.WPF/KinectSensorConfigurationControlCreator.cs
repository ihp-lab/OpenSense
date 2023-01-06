using System.Composition;
using System.Windows;
using OpenSense.Components.Contract;
using OpenSense.Components.Psi.Kinect;
using OpenSense.WPF.Components.Contract;

namespace OpenSense.WPF.Components.Psi.Kinect {
    [Export(typeof(IConfigurationControlCreator))]
    public class KinectSensorConfigurationControlCreator : IConfigurationControlCreator {

        public bool CanCreate(ComponentConfiguration configuration) => configuration is KinectSensorConfiguration;

        public UIElement Create(ComponentConfiguration configuration) => new KinectSensorConfigurationControl() { DataContext = configuration };
    }
}
