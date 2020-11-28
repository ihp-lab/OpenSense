using System.Windows;
using System.Windows.Controls;
using OpenSense.Component.OpenSmile;

namespace OpenSense.Wpf.Component.OpenSmile {
    public partial class OpenSmileConfigurationControl : UserControl {
        public OpenSmileConfigurationControl() {
            InitializeComponent();
        }

        private void ButtonOpenConfigurationFile_Click(object sender, RoutedEventArgs e) {
            var openFileDialog = new Microsoft.Win32.OpenFileDialog {
                CheckFileExists = true,
                AddExtension = true,
                DefaultExt = "*.conf",
                Filter = "OpenSmile Configuration | *.conf",
            };
            if (openFileDialog.ShowDialog() == true) {
                var config = (OpenSmileConfiguration)DataContext;
                config.Raw.ConfigurationFilename = openFileDialog.FileName;
                DataContext = null;//refresh
                DataContext = config;
            }
        }
    }
}
