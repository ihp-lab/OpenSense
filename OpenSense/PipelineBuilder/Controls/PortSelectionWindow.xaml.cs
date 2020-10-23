using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace OpenSense.PipelineBuilder.Controls {
    public partial class PortSelectionWindow : Window {
        public PortSelectionWindow(IList<PortDescription> ports) {
            InitializeComponent();
            DataGridInputs.ItemsSource = ports;
        }

        public PortDescription Result;

        private void ButtonYes_Click(object sender, RoutedEventArgs e) {
            if (DataGridInputs.SelectedItem is PortDescription desc) {
                Result = desc;
                DialogResult = true;
            }
        }
    }
}
