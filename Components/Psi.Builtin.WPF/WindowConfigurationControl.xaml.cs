using System.Windows.Controls;
using OpenSense.Components.Psi;

namespace OpenSense.WPF.Components.Psi {
    public partial class WindowConfigurationControl : UserControl {

        private WindowConfiguration Config => DataContext as WindowConfiguration;

        public WindowConfigurationControl() {
            InitializeComponent();
        }
    }
}
