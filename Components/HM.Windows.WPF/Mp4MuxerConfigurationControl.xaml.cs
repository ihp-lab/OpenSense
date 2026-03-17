#nullable enable

using System.Windows;
using System.Windows.Controls;
using OpenSense.Components.HM;

namespace OpenSense.WPF.Components.HM {
    public sealed partial class Mp4MuxerConfigurationControl : UserControl {

        public Mp4MuxerConfigurationControl() {
            InitializeComponent();
        }

        #region Control Event Handlers
        private void ButtonSaveFile_Click(object sender, RoutedEventArgs e) {
            if (DataContext is not Mp4MuxerConfiguration config) {
                return;
            }
            var saveFileDialog = new Microsoft.Win32.SaveFileDialog {
                AddExtension = true,
                DefaultExt = "mp4",
                Filter = "MP4 Video (*.mp4)|*.mp4|All Files (*.*)|*.*"
            };
            if (saveFileDialog.ShowDialog() == true) {
                config.Filename = saveFileDialog.FileName;
            }
        }
        #endregion
    }
}
