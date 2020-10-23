using System;
using System.Collections.Generic;
using System.IO;
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
using Microsoft.Psi;
using OpenSense.PipelineBuilder.Configurations;

namespace OpenSense.PipelineBuilder.Controls {
    public partial class PipelineEditorWindow : Window {

        public PipelineConfiguration Configuration { get; private set; }

        public PipelineEditorWindow() : this(new PipelineConfiguration()) { }

        public PipelineEditorWindow(PipelineConfiguration config) {
            InitializeComponent();
            Configuration = config;
            UpdateDataContext();
        }

        private void UpdateDataContext() {
            DataContext = Configuration;
            ListBoxInstances.ItemsSource = Configuration.Instances;

            var index = ComboBoxDeliveryPolicy.Items.Cast<ComboBoxItem>().ToList().FindIndex(i => i.Tag as DeliveryPolicy == Configuration?.DeliveryPolicy);
            ComboBoxDeliveryPolicy.SelectedIndex = index >= 0 ? index : 0;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e) {
            
        }

        private void Window_Unloaded(object sender, RoutedEventArgs e) {
            
        }

        private void ComboBoxDeliveryPolicy_SelectionChanged(object sender, SelectionChangedEventArgs e) {
            if (Configuration != null) {
                Configuration.DeliveryPolicy = ((DeliveryPolicy)((ComboBoxItem)ComboBoxDeliveryPolicy.SelectedItem).Tag);
            }
        }

        private void ButtonExecute_Click(object sender, RoutedEventArgs e) {
            if (Configuration is null) {
                return;
            }
            var win = new PipelineExecuterWindow(Configuration);
            win.ShowDialog();
            Configuration = win.Configuration;
        }

        private void ButtonNew_Click(object sender, RoutedEventArgs e) {
            Configuration = new PipelineConfiguration();
            UpdateDataContext();
        }

        private void ButtonAdd_Click(object sender, RoutedEventArgs e) {
            var win = new InstanceTypeSelectionWindow();
            if (win.ShowDialog() == true && win.Result != null) {
                win.Result.Name = ConfigurationManager.Description(win.Result).Name;
                Configuration.Instances.Add(win.Result);
                ListBoxInstances.SelectedItem = win.Result;
            }
        }

        private void ButtonDelete_Click(object sender, RoutedEventArgs e) {
            if (ListBoxInstances.SelectedItem is InstanceConfiguration config) {
                Configuration.Instances.Remove(config);
                ListBoxInstances.SelectedItem = null;
                //remove relative connections
                foreach (var cc in Configuration.Instances) {
                    var toRemove = cc.Inputs.Where(ic => ic.Remote == config.Guid).ToList();
                    foreach (var ic in toRemove) {
                        cc.Inputs.Remove(ic);
                    }
                }
            }
        }

        private void ListBoxInstances_SelectionChanged(object sender, SelectionChangedEventArgs e) {
            var config = (InstanceConfiguration)ListBoxInstances.SelectedItem;
            var instances = (IList<InstanceConfiguration>)ListBoxInstances.ItemsSource;
            ContentControlComponentBasics.Content = null;
            ContentControlConnection.Children.Clear();
            ContentControlConnection.DataContext = config;
            ContentControlSettings.Children.Clear();
            ContentControlSettings.DataContext = ListBoxInstances.SelectedItem;
            if (config != null) {
                ContentControlComponentBasics.Content = new InstanceBasicsControl(config);
                ContentControlConnection.Children.Add(new InstanceConnectionControl(config, instances));
                ContentControlSettings.Children.Add(ControlFactory.CreateConfigurationControl(config, instances));
            }

        }

        private void ButtonLoad_Click(object sender, RoutedEventArgs e) {
            var openFileDialog = new Microsoft.Win32.OpenFileDialog {
                CheckFileExists = true,
                AddExtension = true,
                DefaultExt = "*.pipe.json",
                Filter = "OpenSense Pipeline | *.pipe.json",
            };
            if (openFileDialog.ShowDialog() == true) {
                var json = File.ReadAllText(openFileDialog.FileName);
                Configuration = ConfigurationManager.Deserialize(json);
                UpdateDataContext();
            }
        }

        private void ButtonSave_Click(object sender, RoutedEventArgs e) {
            var saveFileDialog = new Microsoft.Win32.SaveFileDialog {
                AddExtension = true,
                DefaultExt = "*.pipe.json",
                Filter = "OpenSense Pipeline | *.pipe.json",
            };
            if (saveFileDialog.ShowDialog() == true) {
                var json = ConfigurationManager.Serialize(Configuration);
                File.WriteAllText(saveFileDialog.FileName, json);
            }
        }
    }
}
