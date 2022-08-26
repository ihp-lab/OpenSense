using System.Windows.Controls;
using OpenSense.Component.BodyGestureDetectors;

namespace OpenSense.Wpf.Component.BodyGestureDetectors {
    public partial class ArmsProximityDetectorConfigurationControl : UserControl {

        private ArmsProximityDetectorConfiguration Config => DataContext as ArmsProximityDetectorConfiguration;

        public ArmsProximityDetectorConfigurationControl() {
            InitializeComponent();
        }
    }
}
