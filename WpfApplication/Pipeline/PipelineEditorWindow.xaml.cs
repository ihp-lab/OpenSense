using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Psi;
using OpenSense.Components.Contract;
using OpenSense.Pipeline;

namespace OpenSense.WPF.Pipeline {
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
            var win = new CreateComponentConfigurationWindow();
            if (win.ShowDialog() == true && win.Result != null) {
                Configuration.Instances.Add(win.Result);
                ListBoxInstances.SelectedItem = win.Result;
            }
        }

        private void ButtonDelete_Click(object sender, RoutedEventArgs e) {
            if (ListBoxInstances.SelectedItem is ComponentConfiguration config) {
                Configuration.Instances.Remove(config);
                ListBoxInstances.SelectedItem = null;
                //remove relative connections
                foreach (var cc in Configuration.Instances) {
                    var toRemove = cc.Inputs.Where(ic => ic.RemoteId == config.Id).ToList();
                    foreach (var ic in toRemove) {
                        cc.Inputs.Remove(ic);
                    }
                }
            }
        }

        private void ListBoxInstances_SelectionChanged(object sender, SelectionChangedEventArgs e) {
            var configuration = (ComponentConfiguration)ListBoxInstances.SelectedItem;
            var configurations = ListBoxInstances.ItemsSource.Cast<ComponentConfiguration>().ToArray();
            ContentControlComponentBasics.Content = null;
            ContentControlConnection.Children.Clear();
            ContentControlConnection.DataContext = configuration;
            ContentControlSettings.Children.Clear();
            ContentControlSettings.DataContext = ListBoxInstances.SelectedItem;
            if (configuration != null) {
                ContentControlComponentBasics.Content = new InstanceBasicInformationControl(configuration);
                var connection = new InstanceConnectionControl(configuration, configurations);
                ContentControlConnection.Children.Add(connection);
                try {
                    var manager = new ConfigurationControlCreatorManager();//throws ReflectionTypeLoadException
                    var control = manager.Create(configuration);
                    if (control is null) {
                        control = new DefaultConfigurationControl();
                    }
                    ContentControlSettings.Children.Add(control);
                } catch (System.Reflection.ReflectionTypeLoadException refEx) {
                    var errorMessage = "Unable to load the assemblies.";
                    if (refEx.LoaderExceptions.OfType<FileLoadException>().Any()) {
                        errorMessage = errorMessage + "\n\n"
                            + string.Join("\n", refEx.LoaderExceptions.OfType<FileLoadException>().Select(ex => ex.FileName)) + "\n"
                            + "These are because versions of assemblies mismatched. \nPlease verify versions and signatures of the above assemblies.";
                    }
                    if (refEx.LoaderExceptions.OfType<BadImageFormatException>().Any()) {
                        errorMessage = errorMessage + "\n\n"
                            + string.Join("\n", refEx.LoaderExceptions.OfType<BadImageFormatException>().Select(ex => ex.FileName)) + "\n"
                            + "These may because some dependency DLL files are missing. \nPlease verify SDK installtions of the above assemblies.";
                    }
                    errorMessage = errorMessage + "\n\n"
                        + "The following is the raw exception message, listed here for debugging purpose" + "\n"
                        + refEx.ToString();

                    MessageBox.Show(this, errorMessage, "Failed to load assemblies when creating component WPF control", MessageBoxButton.OK, MessageBoxImage.Error);
                }
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
                Configuration = new PipelineConfiguration(json);//TODO: extra json converter
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
                var json = Configuration.ToJson();
                File.WriteAllText(saveFileDialog.FileName, json);
            }
        }
    }
}
