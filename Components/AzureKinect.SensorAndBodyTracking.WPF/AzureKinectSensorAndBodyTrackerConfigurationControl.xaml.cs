using System.Windows.Controls;
using Microsoft.Azure.Kinect.Sensor;

namespace OpenSense.WPF.Components.AzureKinect.SensorAndBodyTracking {
    public sealed partial class AzureKinectSensorAndBodyTrackerConfigurationControl : UserControl {

        public AzureKinectSensorAndBodyTrackerConfigurationControl() {
            InitializeComponent();
        }

        private void UserControl_Loaded(object sender, System.Windows.RoutedEventArgs e) {
            TextBlockInstalledDeviceCount.Text = Device.GetInstalledCount().ToString();
        }
    }
}
