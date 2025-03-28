using System.Windows.Controls;
using Microsoft.Azure.Kinect.Sensor;

namespace OpenSense.WPF.Components.AzureKinect.Sensor {
    public sealed partial class AzureKinectSensorConfigurationControl : UserControl {

        public AzureKinectSensorConfigurationControl() {
            InitializeComponent();
        }

        private void UserControl_Loaded(object sender, System.Windows.RoutedEventArgs e) {
            TextBlockInstalledDeviceCount.Text = Device.GetInstalledCount().ToString();
        }
    }
}
