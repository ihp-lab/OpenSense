using System.Windows.Controls;
using OpenSense.Components.Psi;

namespace OpenSense.WPF.Components.Psi {
    public sealed partial class PsiStoreImporterConfigurationControl : UserControl {

        private PsiStoreImporterConfiguration Configuration => (PsiStoreImporterConfiguration)DataContext;

        public PsiStoreImporterConfigurationControl() {
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
