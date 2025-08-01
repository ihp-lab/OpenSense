#nullable enable

using System.Windows;
using System.Windows.Controls;
using OpenSense.Components.FFMpeg;

namespace OpenSense.WPF.Components.FFMpeg {
    public sealed partial class FileWriterConfigurationControl : UserControl {

        private FileWriterConfiguration? Configuration => (FileWriterConfiguration)DataContext;

        public FileWriterConfigurationControl() {
            InitializeComponent();
        }

        private void ButtonSaveFile_Click(object sender, RoutedEventArgs e) {
            if (Configuration is null) {
                return;
            }
            var saveFileDialog = new Microsoft.Win32.SaveFileDialog {
                AddExtension = true,
                DefaultExt = "mp4",
                Filter = "MP4 Video (*.mp4)|*.mp4|All Files (*.*)|*.*"
            };
            if (saveFileDialog.ShowDialog() == true) {
                Configuration.Filename = saveFileDialog.FileName;
            }
        }
    }
}
