using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using OpenSense.Widget.OpenSmileConfigurationConverter;

namespace OpenSense.WPF.Widget.OpenSmileConfigurationConverter {
    public partial class ConverterWindow : Window {
        public ConverterWindow() {
            InitializeComponent();
        }
        /*
        private string ChooseSaveFilename(string filename) {
            var saveFileDialog = new Microsoft.Win32.SaveFileDialog {
                Title = $"Save modified \"{filename}\" as",
                AddExtension = true,
                DefaultExt = "*.conf",
                Filter = "OpenSmile Configuration | *.conf",
            };
            if (saveFileDialog.ShowDialog() == true) {
                return saveFileDialog.FileName;
            } else {
                return null;
            }
        }
        */

        private static void OpenUrl(string url) {
            var psi = new ProcessStartInfo {
                FileName = url,
                UseShellExecute = true
            };
            try {
                Process.Start(psi);
            } catch (Win32Exception) {

            }
        }

        private void ButtonText_Click(object sender, RoutedEventArgs e) {
            var openFileDialog = new Microsoft.Win32.OpenFileDialog {
                CheckFileExists = true,
                AddExtension = true,
                DefaultExt = "*.conf",
                Filter = "OpenSmile Configuration | *.conf",
            };
            if (openFileDialog.ShowDialog() == true) {
                var saveDir = System.IO.Path.Combine(System.IO.Path.GetDirectoryName(openFileDialog.FileName), "Converted_for_OpenSense");
                Parser.Parse(openFileDialog.FileName, saveDir);
                OpenUrl(saveDir);
            }
            
        }
    }
}
