using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using OpenSense.Component.OpenPose.Visualizer;

namespace OpenSense.WPF.Component.OpenPose.Visualizer {
    public sealed partial class OpenPoseVisualizerInstanceControl : UserControl {
        private OpenPoseVisualizer ViewModel => (OpenPoseVisualizer)DataContext;

        public OpenPoseVisualizerInstanceControl() {
            InitializeComponent();
        }

        private void UserControl_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e) {
            if (e.OldValue is OpenPoseVisualizer old) {
                CompositionTarget.Rendering -= old.RenderingCallback;
            }
            if (e.NewValue is OpenPoseVisualizer @new) {
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
