using System.Windows.Controls;
using OpenSense.Component.BodyGestureDetectors;

namespace OpenSense.Wpf.Component.BodyGestureDetectors {
    public partial class BodyLeaningDetectorConfigurationControl : UserControl {

        private BodyLeaningDetectorConfiguration Config => DataContext as BodyLeaningDetectorConfiguration;

        public BodyLeaningDetectorConfigurationControl() {
            InitializeComponent();
        }
    }
}
