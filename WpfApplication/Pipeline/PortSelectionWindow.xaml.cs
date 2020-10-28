using System.Collections.Generic;
using System.Windows;
using OpenSense.Component.Contract;

namespace OpenSense.Wpf.Pipeline {
    public partial class PortSelectionWindow : Window {
        public PortSelectionWindow(IList<IPortMetadata> ports) {
            InitializeComponent();
            DataGridInputs.ItemsSource = ports;
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
