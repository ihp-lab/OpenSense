using System.Linq;
using System.Windows;
using OpenSense.Component.Contract;
using OpenSense.Pipeline;

namespace OpenSense.Wpf.Pipeline {
    public partial class CreateComponentConfigurationWindow : Window {
        public CreateComponentConfigurationWindow() {
            InitializeComponent();
            var components = new ComponentManager().Components.OrderBy(c => c.Name);
            DataGridComponents.ItemsSource = components;
        }

        public ComponentConfiguration Result { get; private set; }

        private void ButtonYes_Click(object sender, RoutedEventArgs e) {
            if (DataGridComponents.SelectedItem is IComponentMetadata metadata) {
                var config = metadata.CreateConfiguration();
                config.Name = metadata.Name;
                config.Description = metadata.Description;
                Result = config;
                DialogResult = true;
            }
        }
    }
}
