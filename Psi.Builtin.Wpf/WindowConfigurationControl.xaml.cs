using System.Windows.Controls;
using OpenSense.Component.Psi;

namespace OpenSense.Wpf.Component.Psi {
    public partial class WindowConfigurationControl : UserControl {

        private WindowConfiguration Config => DataContext as WindowConfiguration;

        public WindowConfigurationControl() {
            InitializeComponent();
        }
    }
}
