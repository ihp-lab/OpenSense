using System.Windows.Controls;
using System.Windows.Media;
using OpenSense.Component.Builtin.Visualizer;

namespace OpenSense.Wpf.Component.Builtin.Visualizer {
    public partial class DoubleVisualizerInstanceControl : UserControl {
        private DoubleVisualizer ViewModel => (DoubleVisualizer)DataContext;

        public DoubleVisualizerInstanceControl() {
            InitializeComponent();
        }

        private void UserControl_DataContextChanged(object sender, System.Windows.DependencyPropertyChangedEventArgs e) {
            if (e.OldValue is DoubleVisualizer old) {
                CompositionTarget.Rendering -= old.RenderingCallback;
            }
            if (e.NewValue is DoubleVisualizer @new) {
                CompositionTarget.Rendering += @new.RenderingCallback;
            }
        }

        private void UserControl_Unloaded(object sender, System.Windows.RoutedEventArgs e) {
            if (ViewModel != null) {
                CompositionTarget.Rendering -= ViewModel.RenderingCallback;
            }
        }
    }
}
