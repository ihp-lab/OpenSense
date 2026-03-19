#nullable enable

using System.Windows;
using System.Windows.Controls;
using OpenSense.Components.Kvazaar;

namespace OpenSense.WPF.Components.Kvazaar {
    public sealed partial class FileWriterConfigurationControl : UserControl {

        private FileWriterConfiguration? Configuration => (FileWriterConfiguration)DataContext;

        public FileWriterConfigurationControl() {
            InitializeComponent();
#if FIXED_BIT_DEPTH
            InputBitDepthLabel.Visibility = Visibility.Collapsed;
            InputBitDepthControl.Visibility = Visibility.Collapsed;
#else
            InputBitDepthControl.SetBinding(System.Windows.Controls.Primitives.Selector.SelectedValueProperty, new System.Windows.Data.Binding(nameof(FileWriterConfiguration.InputBitDepth)));
#endif
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
