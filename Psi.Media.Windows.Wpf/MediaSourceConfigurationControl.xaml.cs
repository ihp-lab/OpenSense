using System.Windows;
using System.Windows.Controls;
using OpenSense.Component.Psi.Media;

namespace OpenSense.Wpf.Component.Psi.Media {
    public partial class MediaSourceConfigurationControl : UserControl {
        public MediaSourceConfigurationControl() {
            InitializeComponent();
        }

        private void ButtonOpenFile_Click(object sender, RoutedEventArgs e) {
            var openFileDialog = new Microsoft.Win32.OpenFileDialog {
                CheckFileExists = true,
                AddExtension = true,
                DefaultExt = string.Empty,
                Filter = string.Empty,
            };
            if (openFileDialog.ShowDialog() == true) {
                var config = (MediaSourceConfiguration)DataContext;
                config.Filename = openFileDialog.FileName;
            }
        }
    }
}
