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
using System.Windows.Navigation;
using System.Windows.Shapes;
using Microsoft.Psi;

namespace OpenSense.PipelineBuilder.Controls {
    public partial class InstanceConnectionControl : UserControl {

        private InstanceConfiguration Config;

        private ComponentDescription Desc => ConfigurationManager.Description(Config);

        private IList<InstanceConfiguration> Instances;

        public InstanceConnectionControl(InstanceConfiguration config, IList<InstanceConfiguration> instances) {
            InitializeComponent();
            Config = config;
            Instances = instances.ToList();
            DataContext = config;
        }

        private void ButtonAdd_Click(object sender, RoutedEventArgs e) {
            var filteredInputs = Desc.Inputs.Where(p => (p.IsDictionary || p.IsList) || Config.Inputs.All(c => c.Input.PropertyName != p.Name)).ToList();
            if (filteredInputs.Count == 0) {
                MessageBox.Show("No floating input port");
                return;
            }
            var win = new PortSelectionWindow(filteredInputs);
            if (win.ShowDialog() == true && win.Result != null) {
                var portDesc = win.Result;
                var config = new InputConfiguration() {
                    Input = new PortConfiguration() {
                        PropertyName = portDesc.Name,
                        Indexer = portDesc.IsList || portDesc.IsDictionary ? string.Empty : null,
                    },
                };
                Config.Inputs.Add(config);
                ListBoxInputs.SelectedItem = config;
            }
        }

        private void ButtonDelete_Click(object sender, RoutedEventArgs e) {
            if (ListBoxInputs.SelectedItem is InputConfiguration config) {
                Config.Inputs.Remove(config);
            }
        }

        private void ListBoxInputs_SelectionChanged(object sender, SelectionChangedEventArgs e) {
            ContentControlConnection.Children.Clear();
            var config = (InputConfiguration)ListBoxInputs.SelectedItem;
            if (config != null) {
                var control = new OutputSelectionControl(Config, config, Instances);
                ContentControlConnection.Children.Add(control);
            }
            ComboBoxDeliveryPolicy.SelectedItem = ComboBoxDeliveryPolicy.Items.Cast<ComboBoxItem>().Single(i => i.Tag as DeliveryPolicy == config?.DeliveryPolicy);
        }

        private void ComboBoxDeliveryPolicy_SelectionChanged(object sender, SelectionChangedEventArgs e) {
            var config = (InputConfiguration)ListBoxInputs.SelectedItem;
            if (config != null) {
                config.DeliveryPolicy = ((DeliveryPolicy)((ComboBoxItem)ComboBoxDeliveryPolicy.SelectedItem).Tag);
            }
        }
    }
}
