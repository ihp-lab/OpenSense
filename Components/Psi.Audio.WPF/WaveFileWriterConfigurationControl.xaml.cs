using System.Windows;
using System.Windows.Controls;
using OpenSense.Components.Psi.Audio;

namespace OpenSense.WPF.Components.Psi.Audio {
    public partial class WaveFileWriterConfigurationControl : UserControl {
        public WaveFileWriterConfigurationControl() {
            InitializeComponent();
        }

        private void ButtonOpenFile_Click(object sender, RoutedEventArgs e) {
            var saveFileDialog = new Microsoft.Win32.SaveFileDialog {
                AddExtension = true,
                DefaultExt = "*.wav",
                Filter = "Wave Audio | *.wav",
            };
            if (saveFileDialog.ShowDialog() == true) {
                var config = (WaveFileWriterConfiguration)DataContext;
                config.Filename = saveFileDialog.FileName;
            }
        }
    }
}
