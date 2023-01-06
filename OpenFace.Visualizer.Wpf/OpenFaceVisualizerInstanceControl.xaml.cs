using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using OpenSense.Component.OpenFace.Visualizer;

namespace OpenSense.WPF.Component.OpenFace.Visualizer {
    public sealed partial class OpenFaceVisualizerInstanceControl : UserControl {
        private OpenFaceVisualizer ViewModel => (OpenFaceVisualizer)DataContext;

        public OpenFaceVisualizerInstanceControl() {
            InitializeComponent();
        }

        private void UserControl_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e) {
            if (e.OldValue is OpenFaceVisualizer old) {
                CompositionTarget.Rendering -= old.RenderingCallback;
            }
            if (e.NewValue is OpenFaceVisualizer @new) {
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
