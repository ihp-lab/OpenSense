using System.Windows.Controls;
using OpenSense.Components.Psi.Data;

namespace OpenSense.WPF.Components.Psi.Data {
    public sealed partial class JsonStoreExporterConfigurationControl : UserControl {

        private JsonStoreExporterConfiguration Configuration => (JsonStoreExporterConfiguration)DataContext;

        public JsonStoreExporterConfigurationControl() {
            InitializeComponent();
        }

        private void ButtonOpen_Click(object sender, System.Windows.RoutedEventArgs e) {
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
