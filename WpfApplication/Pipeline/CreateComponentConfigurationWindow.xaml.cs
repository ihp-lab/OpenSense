using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using OpenSense.Components.Contract;
using OpenSense.Pipeline;

namespace OpenSense.WPF.Pipeline {
    public partial class CreateComponentConfigurationWindow : Window {

        private IComponentMetadata[] components;

        public CreateComponentConfigurationWindow() {
            InitializeComponent();
            components = new ComponentManager().Components.OrderBy(c => c.Name).ToArray();
            DataGridComponents.ItemsSource = components;
        }

        public ComponentConfiguration Result { get; private set; }

        private void ButtonYes_Click(object sender, RoutedEventArgs e) {
            if (DataGridComponents.SelectedItem is IComponentMetadata metadata) {
                var config = metadata.CreateConfiguration();
                config.Name = metadata.Name;
                config.Description = "";
                Result = config;
                DialogResult = true;
            }
        }

        private void TextBoxFilter_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e) {
            IComponentMetadata[] data;
            var text = TextBoxFilter.Text.Trim();
            if (string.IsNullOrEmpty(text)) {
                data = components;
            } else {
                var tokens = text.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                data = components.Where(c => tokens.All(t => c.Name.Contains(t, StringComparison.InvariantCultureIgnoreCase)))
                    .ToArray();
            }
            var previousLength = (DataGridComponents.ItemsSource as IReadOnlyCollection<IComponentMetadata>)?.Count;
            DataGridComponents.ItemsSource = data;
            if (data.Length > 0 && previousLength.HasValue && data.Length != previousLength.Value) {
                DataGridComponents.SelectedIndex = 0;
            }
        }
    }
}
