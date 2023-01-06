using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace OpenSense.WPF.Component.MediaPipe.NET.Visualizer {
    public partial class NormalizedLandmarkListVectorVisualizerInstanceControl : UserControl {

        private NormalizedLandmarkListVectorVisualizer ViewModel => (NormalizedLandmarkListVectorVisualizer)DataContext;

        public NormalizedLandmarkListVectorVisualizerInstanceControl() {
            InitializeComponent();
        }

        private void UserControl_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e) {
            if (e.OldValue is NormalizedLandmarkListVectorVisualizer old) {
                CompositionTarget.Rendering -= old.RenderingCallback;
            }
            if (e.NewValue is NormalizedLandmarkListVectorVisualizer @new) {
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
