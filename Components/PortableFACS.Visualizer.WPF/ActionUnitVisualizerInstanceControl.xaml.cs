using System.Windows.Controls;
using OpenSense.Components.PortableFACS.Visualizer;

namespace OpenSense.WPF.Components.PortableFACS {
    public partial class ActionUnitVisualizerInstanceControl : UserControl {

        private ActionUnitVisualizer Instance => DataContext as ActionUnitVisualizer;

        public ActionUnitVisualizerInstanceControl() {
            InitializeComponent();
        }
    }
}
