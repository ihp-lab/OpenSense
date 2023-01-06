using System.Windows.Controls;
using OpenSense.Components.Psi.Data;

namespace OpenSense.WPF.Components.Psi.Data {
    public partial class JsonStoreExporterConfigurationControl : UserControl {
        private JsonStoreExporterConfiguration ViewModel => (JsonStoreExporterConfiguration)DataContext;

        public JsonStoreExporterConfigurationControl() {
            InitializeComponent();
        }

    }
}
