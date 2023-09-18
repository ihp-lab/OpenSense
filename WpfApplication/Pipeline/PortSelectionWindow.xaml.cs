using System.Collections.Generic;
using System.Windows;
using OpenSense.Components;

namespace OpenSense.WPF.Pipeline {
    public partial class PortSelectionWindow : Window {
        public PortSelectionWindow(IList<IPortMetadata> ports) {
            InitializeComponent();
            DataGridInputs.ItemsSource = ports;
            if (ports.Count > 0) {
                DataGridInputs.SelectedIndex = 0;
            }
        }

        public IPortMetadata Result { get; private set; }

        private void ButtonYes_Click(object sender, RoutedEventArgs e) {
            if (DataGridInputs.SelectedItem is IPortMetadata metadata) {
                Result = metadata;
                DialogResult = true;
            }
        }
    }
}
