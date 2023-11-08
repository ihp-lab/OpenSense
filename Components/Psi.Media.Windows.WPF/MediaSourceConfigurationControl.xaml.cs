using System.IO;
using System.Windows;
using System.Windows.Controls;
using OpenSense.Components.Psi.Media;

namespace OpenSense.WPF.Components.Psi.Media {
    public sealed partial class MediaSourceConfigurationControl : UserControl {

        private MediaSourceConfiguration Configuration => (MediaSourceConfiguration)DataContext;

        public MediaSourceConfigurationControl() {
            InitializeComponent();
        }

        private void ButtonOpenFile_Click(object sender, RoutedEventArgs e) {
            var openFileDialog = new Microsoft.Win32.OpenFileDialog {
                CheckFileExists = true,
                AddExtension = true,
                DefaultExt = "*.json",
                Filter = "MPEG4 (*.mp4) | *.mp4",
                InitialDirectory = string.IsNullOrEmpty(Configuration.Filename) ? "" : Path.GetDirectoryName(Path.GetFullPath(Configuration.Filename)),
            };
            if (openFileDialog.ShowDialog() == true) {
                Configuration.Filename = openFileDialog.FileName;
            }
        }
    }
}
