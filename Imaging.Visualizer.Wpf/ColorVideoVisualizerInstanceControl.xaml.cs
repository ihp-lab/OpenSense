using System.Windows.Controls;

namespace OpenSense.Component.Imaging.Visualizer {
    public partial class ColorVideoVisualizerInstanceControl : UserControl {
        public ColorVideoVisualizerInstanceControl(ColorVideoVisualizer visualizer) {
            InitializeComponent();
            DataContext = visualizer;
        }
    }
}
