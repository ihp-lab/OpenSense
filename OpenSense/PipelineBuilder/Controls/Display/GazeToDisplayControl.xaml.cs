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
using OpenSense.Components;
using OpenSense.GazeToDisplayConverter;

namespace OpenSense.PipelineBuilder.Controls.Display {
    public partial class GazeToDisplayControl : UserControl {
        public GazeToDisplayControl() {
            InitializeComponent();
        }

        private void ButtonSetConverter_Click(object sender, RoutedEventArgs e) {
            var openFileDialog = new Microsoft.Win32.OpenFileDialog {
                CheckFileExists = true,
                AddExtension = true,
                DefaultExt = "*.json",
                Filter = "JSON | *.json",
            };
            if (openFileDialog.ShowDialog() == true) {
                var component = (GazeToDisplay)DataContext;
                try {
                    component.Converter = GazeToDisplayConverterHelper.Load(openFileDialog.FileName);
                    TextBlockConverterNotification.Visibility = Visibility.Collapsed;
                    MessageBox.Show("Loaded");
                } catch (Exception ex) {
                    MessageBox.Show(ex.ToString(), "Failed to load converter", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
    }
}
