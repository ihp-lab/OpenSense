using System;
using System.Collections.Generic;
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
using System.Windows.Navigation;
using System.Windows.Shapes;
using OpenSense.PipelineBuilder.Configurations;

namespace OpenSense.PipelineBuilder.Controls.Configuration {
    public partial class OpenSmileControl : UserControl {
        public OpenSmileControl() {
            InitializeComponent();
        }

        private void ButtonOpenConfigurationFile_Click(object sender, RoutedEventArgs e) {
            var openFileDialog = new Microsoft.Win32.OpenFileDialog {
                CheckFileExists = true,
                AddExtension = true,
                DefaultExt = "*.conf",
                Filter = "OpenSmile Configuration | *.conf",
            };
            if (openFileDialog.ShowDialog() == true) {
                var config = (OpenSmileConfiguration)DataContext;
                config.Raw.ConfigurationFilename = openFileDialog.FileName;
                DataContext = null;//refresh
                DataContext = config;
            }
        }
    }
}
