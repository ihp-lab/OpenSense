using System.Windows.Controls;
using OpenSense.Component.BodyGestureDetectors;

namespace OpenSense.WPF.Component.BodyGestureDetectors {
    public partial class BodyAttitudeDetectorConfigurationControl : UserControl {

        private BodyAttitudeDetectorConfiguration Config => DataContext as BodyAttitudeDetectorConfiguration;

        public BodyAttitudeDetectorConfigurationControl() {
            InitializeComponent();
        }
    }
}
