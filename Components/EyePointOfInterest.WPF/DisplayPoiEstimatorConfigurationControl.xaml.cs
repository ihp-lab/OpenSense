using System.Windows;
using System.Windows.Controls;
using OpenSense.Components.EyePointOfInterest;

namespace OpenSense.WPF.Components.EyePointOfInterest {
    public partial class DisplayPoiEstimatorConfigurationControl : UserControl {

        private DisplayPoiEstimatorConfiguration Config => DataContext as DisplayPoiEstimatorConfiguration;

        public DisplayPoiEstimatorConfigurationControl() {
            InitializeComponent();
        }

        private void ButtonOpenEstimatorConfigurationFilename_Click(object sender, RoutedEventArgs e) {
            var openFileDialog = new Microsoft.Win32.OpenFileDialog {
                CheckFileExists = true,
                AddExtension = true,
                DefaultExt = "*.poi_param.json",
                Filter = "POI on Display Estimator | *.poi_param.json",
            };
            if (openFileDialog.ShowDialog() == true) {
                Config.EstimatorConfigurationFilename = openFileDialog.FileName;
            }
        }
    }
}
