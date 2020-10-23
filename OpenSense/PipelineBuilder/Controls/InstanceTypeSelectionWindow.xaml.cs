using System;
using System.Collections.Generic;
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
    public partial class InstanceTypeSelectionWindow : Window {
        public InstanceTypeSelectionWindow() {
            InitializeComponent();
            DataGridComponents.ItemsSource = ConfigurationManager.Components;
        }

        public InstanceConfiguration Result;

        private void ButtonYes_Click(object sender, RoutedEventArgs e) {
            if (DataGridComponents.SelectedItem is ComponentDescription desc) {
                Result = (InstanceConfiguration)desc.ConfigurationType.GetConstructor(Type.EmptyTypes).Invoke(null);
                DialogResult = true;
            }
        }
    }
}
