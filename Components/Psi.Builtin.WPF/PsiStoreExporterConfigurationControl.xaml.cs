using System;
using System.Collections.Specialized;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using OpenSense.Components;
using OpenSense.Components.Psi;

namespace OpenSense.WPF.Components.Psi {
    public sealed partial class PsiStoreExporterConfigurationControl : UserControl {

        private PsiStoreExporterConfiguration Configuration => (PsiStoreExporterConfiguration)DataContext;

        public PsiStoreExporterConfigurationControl() {
            InitializeComponent();
        }

        private void UserControl_Loaded(object sender, System.Windows.RoutedEventArgs e) {
            Configuration.Inputs.CollectionChanged += OnInputCollectionChanged;
            RefreshLargeMessageList();
        }

        private void UserControl_Unloaded(object sender, System.Windows.RoutedEventArgs e) {
            Configuration.Inputs.CollectionChanged -= OnInputCollectionChanged;
        }

        private void OnInputCollectionChanged(object sender, NotifyCollectionChangedEventArgs args) {
            RefreshLargeMessageList();
        }

        private void RefreshLargeMessageList() {
            StackPanelLargeMessage.Children.Clear();
            foreach (var inputConfig in Configuration.Inputs) {
                var binding = new Binding(nameof(inputConfig.LocalPort.Index)) { Source = inputConfig.LocalPort, UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged };
                var checkbox = new CheckBox() { IsChecked = Configuration.LargeMessageInputs.Contains(inputConfig.Id), Tag = inputConfig };
                checkbox.SetBinding(CheckBox.ContentProperty, binding);
                checkbox.Checked += OnLargeMessageCheckBoxChecked;
                checkbox.Unchecked += OnLargeMessageCheckBoxUnchecked;
                StackPanelLargeMessage.Children.Add(checkbox);
            }
            StackPanelLargeMessage.Visibility = StackPanelLargeMessage.Children.Count != 0 ? Visibility.Visible : Visibility.Hidden;
            TextBlockLargeMessageNoStream.Visibility = StackPanelLargeMessage.Children.Count == 0 ? Visibility.Visible : Visibility.Hidden;
        }

        private void OnLargeMessageCheckBoxChecked(object sender, EventArgs args) {
            var config = (InputConfiguration)((CheckBox)sender).Tag;
            Configuration.LargeMessageInputs.Add(config.Id);

        }

        private void OnLargeMessageCheckBoxUnchecked(object sender, EventArgs args) {
            var config = (InputConfiguration)((CheckBox)sender).Tag;
            Configuration.LargeMessageInputs.Remove(config.Id);
        }

        private void ButtonOpen_Click(object sender, RoutedEventArgs e) {
            var dialog = new System.Windows.Forms.FolderBrowserDialog() {
                ShowNewFolderButton = true,
                Description = "Select a Folder",
            };
            if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK) {
                Configuration.RootPath = dialog.SelectedPath;
            }
        }
    }
}
