using System.IO;
using System.Windows;
using System.Windows.Controls;
using OpenSense.Components.Psi.Media;

namespace OpenSense.WPF.Components.Psi.Media {
    public sealed partial class Mpeg4WriterConfigurationControl : UserControl {

        private Mpeg4WriterConfiguration Configuration => (Mpeg4WriterConfiguration)DataContext;

        public Mpeg4WriterConfigurationControl() {
            InitializeComponent();
        }

        private void ButtonOpenFile_Click(object sender, RoutedEventArgs e) {
            var saveFileDialog = new Microsoft.Win32.SaveFileDialog {
                AddExtension = true,
                DefaultExt = "*.mp4",
                Filter = "MPEG4 (*.mp4) | *.mp4",
                InitialDirectory = string.IsNullOrEmpty(Configuration.Filename) ? "" : Path.GetDirectoryName(Path.GetFullPath(Configuration.Filename)),
            };
            if (saveFileDialog.ShowDialog() == true) {
                Configuration.Filename = saveFileDialog.FileName;
            }
        }
    }
}
