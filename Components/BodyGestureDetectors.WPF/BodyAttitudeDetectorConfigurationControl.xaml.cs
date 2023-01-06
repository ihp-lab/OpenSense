using System.Windows.Controls;
using OpenSense.Components.BodyGestureDetectors;

namespace OpenSense.WPF.Components.BodyGestureDetectors {
    public partial class BodyAttitudeDetectorConfigurationControl : UserControl {

        private BodyAttitudeDetectorConfiguration Config => DataContext as BodyAttitudeDetectorConfiguration;

        public BodyAttitudeDetectorConfigurationControl() {
            InitializeComponent();
        }
    }
}
