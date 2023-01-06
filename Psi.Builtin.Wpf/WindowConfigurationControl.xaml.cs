using System.Windows.Controls;
using OpenSense.Component.Psi;

namespace OpenSense.WPF.Component.Psi {
    public partial class WindowConfigurationControl : UserControl {

        private WindowConfiguration Config => DataContext as WindowConfiguration;

        public WindowConfigurationControl() {
            InitializeComponent();
        }
    }
}
