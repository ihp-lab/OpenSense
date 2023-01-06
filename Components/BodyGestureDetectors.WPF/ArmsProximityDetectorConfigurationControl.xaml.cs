using System.Windows.Controls;
using OpenSense.Components.BodyGestureDetectors;

namespace OpenSense.WPF.Components.BodyGestureDetectors {
    public partial class ArmsProximityDetectorConfigurationControl : UserControl {

        private ArmsProximityDetectorConfiguration Config => DataContext as ArmsProximityDetectorConfiguration;

        public ArmsProximityDetectorConfigurationControl() {
            InitializeComponent();
        }
    }
}
