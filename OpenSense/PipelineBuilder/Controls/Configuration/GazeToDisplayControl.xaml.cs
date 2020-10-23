using System.Windows;
using System.Windows.Controls;
using OpenSense.PipelineBuilder.Configurations;

namespace OpenSense.PipelineBuilder.Controls.Configuration {
    public partial class GazeToDisplayControl : UserControl {
        public GazeToDisplayControl() {
            InitializeComponent();
        }

        private void ButtonOpenConverterFilename_Click(object sender, RoutedEventArgs e) {
            var openFileDialog = new Microsoft.Win32.OpenFileDialog {
                CheckFileExists = true,
                AddExtension = true,
                DefaultExt = "*.gaze_conv.json",
                Filter = "Gaze Converter | *.gaze_conv.json",
            };
            if (openFileDialog.ShowDialog() == true) {
                var config = (GazeToDisplayConfiguration)DataContext;
                config.ConverterFilename = openFileDialog.FileName;
                DataContext = null;//refresh
                DataContext = config;
            }
        }
    }
}
