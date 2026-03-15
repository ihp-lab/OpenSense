#nullable enable

using System.Windows;
using System.Windows.Controls;
using OpenSense.Components.HM;

namespace OpenSense.WPF.Components.HM {
    public sealed partial class FileReaderConfigurationControl : UserControl {

        public FileReaderConfigurationControl() {
            InitializeComponent();
        }

        #region Control Event Handlers
        private void ButtonOpenFile_Click(object sender, RoutedEventArgs e) {
            if (DataContext is not FileReaderConfiguration config) {
                return;
            }
            var openFileDialog = new Microsoft.Win32.OpenFileDialog {
                CheckFileExists = true,
                AddExtension = true,
                DefaultExt = "mp4",
                Filter = "MP4 Video (*.mp4)|*.mp4|All Files (*.*)|*.*",
            };
            if (openFileDialog.ShowDialog() == true) {
                config.Filename = openFileDialog.FileName;
            }
        }
        #endregion
    }
}
