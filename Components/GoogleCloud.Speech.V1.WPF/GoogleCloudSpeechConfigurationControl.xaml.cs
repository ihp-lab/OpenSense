using System.IO;
using System.Windows;
using System.Windows.Controls;
using OpenSense.Components.GoogleCloud.Speech.V1;

namespace OpenSense.WPF.Components.GoogleCloud.Speech.V1 {
    public sealed partial class GoogleCloudSpeechConfigurationControl : UserControl {

        private GoogleCloudSpeechConfiguration Configuration => (GoogleCloudSpeechConfiguration)DataContext;

		public GoogleCloudSpeechConfigurationControl() {
			InitializeComponent();
		}

        private void ButtonOpenJsonContent_Click(object sender, RoutedEventArgs e) {
            var openFileDialog = new Microsoft.Win32.OpenFileDialog {
                CheckFileExists = true,
                AddExtension = true,
                DefaultExt = "*.json",
                Filter = "JSON (*.json) | *.json | Text (*.txt) | *.txt | All (*.*) | *.*",
                InitialDirectory = string.IsNullOrEmpty(Configuration.Credentials) ? "" : Path.GetDirectoryName(Path.GetFullPath(Configuration.Credentials)),
            };
            if (openFileDialog.ShowDialog() == true) {
                Configuration.Credentials = File.ReadAllText(openFileDialog.FileName);
            }
        }

        private void ButtonOpenFilePath_Click(object sender, RoutedEventArgs e) {
            var openFileDialog = new Microsoft.Win32.OpenFileDialog {
                CheckFileExists = true,
                AddExtension = true,
                DefaultExt = "*.json",
                Filter = "JSON (*.json) | *.json | Text (*.txt) | *.txt | All (*.*) | *.*",
                InitialDirectory = string.IsNullOrEmpty(Configuration.CredentialsPath) ? "" : Path.GetDirectoryName(Path.GetFullPath(Configuration.CredentialsPath)),
            };
            if (openFileDialog.ShowDialog() == true) {
                Configuration.CredentialsPath = openFileDialog.FileName;
            }
        }
    }
}
