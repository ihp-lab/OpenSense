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
    public partial class Mpeg4WriterControl : UserControl {
        public Mpeg4WriterControl() {
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
