using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Psi;
using OpenSense.Components;
using OpenSense.WPF.Views;

namespace OpenSense.WPF.Pipeline {
    public partial class InstanceConnectionControl : UserControl {

        private ComponentConfiguration Configuration;

        private IReadOnlyList<ComponentConfiguration> Configurations;

        public InstanceConnectionControl(ComponentConfiguration configuration, IReadOnlyList<ComponentConfiguration> configurations) {
            InitializeComponent();
            Configuration = configuration;
            Configurations = configurations;
            DataContext = configuration;
        }

        private void ButtonAdd_Click(object sender, RoutedEventArgs e) {
            var metadata = Configuration.GetMetadata();
            List<IPortMetadata> filteredInputs;
            try {
                filteredInputs = metadata.InputPorts()
                .Where(p => p.Aggregation != PortAggregation.Object || Configuration.Inputs.All(c => !Equals(c.LocalPort.Identifier, p.Identifier)))
                .ToList();
            } catch (Exception ex) {
                ExceptionMessageWindow.Show(this, ex, "Port Evaluation Error", "An error occurred while evaluating port definitions.");
                return;
            }
            if (filteredInputs.Count == 0) {
                MessageBox.Show("No floating input port");
                return;
            }
            var win = new PortSelectionWindow(filteredInputs);
            if (win.ShowDialog() == true && win.Result != null) {
                var portMetadata = win.Result;
                var config = new InputConfiguration() {
                    LocalPort = new PortConfiguration() {
                        Identifier = portMetadata.Identifier,
                        Index = portMetadata.Aggregation.DefaultPortIndex(),
                    },
                };
                Configuration.Inputs.Add(config);
                ListBoxInputs.SelectedItem = config;
            }
        }

        private void ButtonDelete_Click(object sender, RoutedEventArgs e) {
            if (ListBoxInputs.SelectedItem is InputConfiguration config) {
                Configuration.Inputs.Remove(config);
            }
        }

        private void ListBoxInputs_SelectionChanged(object sender, SelectionChangedEventArgs e) {//TODO: Use template, do not new controls.
            ContentControlConnection.Children.Clear();
            var config = (InputConfiguration)ListBoxInputs.SelectedItem;
            if (config != null) {
                try {
                    var control = new OutputSelectionControl(Configuration, config, Configurations);
                    ContentControlConnection.Children.Add(control);
                } catch (Exception ex) {
                    ContentControlConnection.Children.Add(new ErrorOccurredOutputSelectionControl());
                    ExceptionMessageWindow.Show(this, ex, "Port Evaluation Error", "An error occurred while evaluating port definitions.");
                }
            }
            ComboBoxDeliveryPolicy.SelectedItem = ComboBoxDeliveryPolicy.Items.Cast<ComboBoxItem>().Single(i => i.Tag as DeliveryPolicy == config?.DeliveryPolicy);
        }

        private void ComboBoxDeliveryPolicy_SelectionChanged(object sender, SelectionChangedEventArgs e) {
            var config = (InputConfiguration)ListBoxInputs.SelectedItem;
            if (config is null) {
                return;
            }
            config.DeliveryPolicy = ((DeliveryPolicy)((ComboBoxItem)ComboBoxDeliveryPolicy.SelectedItem).Tag);
        }
    }
}
