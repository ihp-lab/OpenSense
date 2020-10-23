using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using OpenSense.Utilities;

namespace OpenSense.OpenSmileConfigurationConverter {
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
                OpenUrl.Open(saveDir);
            }
            
        }
    }
}
