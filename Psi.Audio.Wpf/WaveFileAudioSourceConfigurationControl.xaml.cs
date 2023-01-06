using System.Windows;
using System.Windows.Controls;
using OpenSense.Component.Psi.Audio;

namespace OpenSense.WPF.Component.Psi.Audio {
    public partial class WaveFileAudioSourceConfigurationControl : UserControl {
        public WaveFileAudioSourceConfigurationControl() {
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
                var config = (WaveFileAudioSourceConfiguration)DataContext;
                config.Filename = openFileDialog.FileName;
            }
        }
    }
}
