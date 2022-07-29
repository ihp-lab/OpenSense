using System.Windows.Controls;
using OpenSense.Component.BodyGestureDetectors;

namespace OpenSense.Wpf.Component.BodyGestureDetectors {
    public partial class BodySwirlDetectorConfigurationControl : UserControl {

        private BodySwirlDetectorConfiguration Config => DataContext as BodySwirlDetectorConfiguration;

        public BodySwirlDetectorConfigurationControl() {
            InitializeComponent();
        }
    }
}
