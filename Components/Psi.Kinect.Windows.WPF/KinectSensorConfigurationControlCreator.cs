using System.Composition;
using System.Windows;
using OpenSense.Components;
using OpenSense.Components.Psi.Kinect;

namespace OpenSense.WPF.Components.Psi.Kinect {
    [Export(typeof(IConfigurationControlCreator))]
    public class KinectSensorConfigurationControlCreator : IConfigurationControlCreator {

        public bool CanCreate(ComponentConfiguration configuration) => configuration is KinectSensorConfiguration;

        public UIElement Create(ComponentConfiguration configuration) => new KinectSensorConfigurationControl() { DataContext = configuration };
    }
}
