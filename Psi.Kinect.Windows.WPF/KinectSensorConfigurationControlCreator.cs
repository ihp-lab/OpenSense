using System.Composition;
using System.Windows;
using OpenSense.Component.Contract;
using OpenSense.Component.Psi.Kinect;
using OpenSense.WPF.Component.Contract;

namespace OpenSense.WPF.Component.Psi.Kinect {
    [Export(typeof(IConfigurationControlCreator))]
    public class KinectSensorConfigurationControlCreator : IConfigurationControlCreator {

        public bool CanCreate(ComponentConfiguration configuration) => configuration is KinectSensorConfiguration;

        public UIElement Create(ComponentConfiguration configuration) => new KinectSensorConfigurationControl() { DataContext = configuration };
    }
}
