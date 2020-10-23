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
using OpenSense.Utilities.DataWriter;

namespace OpenSense.PipelineBuilder.Controls {
    public partial class DataWriterConfigurationControl : UserControl {

        public DataWriterConfigurationControl() {
            InitializeComponent();
        }

        private void ButtonOpen_Click(object sender, RoutedEventArgs e) {
            var saveFileDialog = new Microsoft.Win32.SaveFileDialog {
                AddExtension = true,
                DefaultExt = "*.csv",
                Filter = "Comma-separated values | *.csv",
            };
            if (saveFileDialog.ShowDialog() == true) {
                var config = (DataWriterConfiguration)DataContext;
                config.Filename = saveFileDialog.FileName;
            }
        }
    }
}
