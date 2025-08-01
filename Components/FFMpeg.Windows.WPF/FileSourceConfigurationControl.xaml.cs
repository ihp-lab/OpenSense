#nullable enable

using System.Windows;
using System.Windows.Controls;
using OpenSense.Components.FFMpeg;

namespace OpenSense.WPF.Components.FFMpeg {
    public sealed partial class FileSourceConfigurationControl : UserControl {

        private FileSourceConfiguration? Configuration => (FileSourceConfiguration)DataContext;

        public FileSourceConfigurationControl() {
            InitializeComponent();
        }

        private void ButtonOpenFile_Click(object sender, RoutedEventArgs e) {
            if (Configuration is null) {
                return;
            }
            var openFileDialog = new Microsoft.Win32.OpenFileDialog {
                CheckFileExists = true,
                AddExtension = true,
                DefaultExt = "*.*",
                Filter = "All Files (*.*)|*.*",
            };
            if (openFileDialog.ShowDialog() == true) {
                Configuration.Filename = openFileDialog.FileName;
            }
        }
    }
}
