using System.Windows.Controls;
using OpenSense.Components.Psi.Data;

namespace OpenSense.WPF.Components.Psi.Data {
    public sealed partial class JsonStoreImporterConfigurationControl : UserControl {

        private JsonStoreImporterConfiguration Configuration => (JsonStoreImporterConfiguration)DataContext;

        public JsonStoreImporterConfigurationControl() {
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
