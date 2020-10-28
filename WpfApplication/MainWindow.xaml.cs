using System.Windows;
using OpenSense.Wpf.Pipeline;

namespace OpenSense.Wpf {

    public partial class MainWindow : Window {
        public MainWindow() {
            InitializeComponent();
        }

        private void ButtonPipelineEditor_Click(object sender, RoutedEventArgs e) {
            var editor = new PipelineEditorWindow();
            editor.Show();
        }

        private void ButtonPipelineExecuter_Click(object sender, RoutedEventArgs e) {
            var executor = new PipelineExecuterWindow();
            executor.Show();
        }

        private void ButtonGazeCalibrator_Click(object sender, RoutedEventArgs e) {
            //var calibrator = new CalibratorWindow();
            //calibrator.ShowDialog();
        }

        private void ButtonOpenSmileConfigurationConverter_Click(object sender, RoutedEventArgs e) {
            //var converter = new OpenSmileConfigurationConverter.ConverterWindow();
            //converter.ShowDialog();
        }
    }
}
