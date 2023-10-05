using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using OpenSense.Components.Psi.Imaging.Visualizer;

namespace OpenSense.WPF.Components.Psi.Imaging.Visualizer {
    public sealed partial class DepthImageVisualizerInstanceControl : UserControl {
        
        private DepthImageVisualizer ViewModel => (DepthImageVisualizer)DataContext;

        public DepthImageVisualizerInstanceControl() {
            InitializeComponent();
        }

        private void UserControl_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e) {
            if (e.OldValue is DepthImageVisualizer old) {
                CompositionTarget.Rendering -= old.RenderingCallback;
            }
            if (e.NewValue is DepthImageVisualizer @new) {
                CompositionTarget.Rendering += @new.RenderingCallback;//will be called before every time WPF wants to render a frame
            }
        }

        private void UserControl_Unloaded(object sender, RoutedEventArgs e) {
            if (ViewModel != null) {
                CompositionTarget.Rendering -= ViewModel.RenderingCallback;
            }
        }
    }
}
