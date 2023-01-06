using System.Windows;
using System.Windows.Controls;
using OpenSense.Component.Psi.Media;

namespace OpenSense.WPF.Component.Psi.Media {
    public partial class Mpeg4WriterConfigurationControl : UserControl {
        public Mpeg4WriterConfigurationControl() {
            InitializeComponent();
        }

        private void ButtonOpenFile_Click(object sender, RoutedEventArgs e) {
            var saveFileDialog = new Microsoft.Win32.SaveFileDialog {
                AddExtension = true,
                DefaultExt = "*.mp4",
                Filter = "mp4 | *.mp4",
            };
            if (saveFileDialog.ShowDialog() == true) {
                var config = (Mpeg4WriterConfiguration)DataContext;
                config.Filename = saveFileDialog.FileName;
            }
        }
    }
}
