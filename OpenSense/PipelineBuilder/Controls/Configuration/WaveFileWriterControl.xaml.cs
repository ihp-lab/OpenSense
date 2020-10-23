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
    public partial class WaveFileWriterControl : UserControl {
        public WaveFileWriterControl() {
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
