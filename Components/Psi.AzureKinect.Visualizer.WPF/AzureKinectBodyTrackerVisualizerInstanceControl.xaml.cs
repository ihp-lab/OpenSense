using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using OpenSense.Components.Psi.AzureKinect.Visualizer;

namespace OpenSense.WPF.Components.Psi.AzureKinect.Visualizer {
    public partial class AzureKinectBodyTrackerVisualizerInstanceControl : UserControl {

        private AzureKinectBodyTrackerVisualizer ViewModel => (AzureKinectBodyTrackerVisualizer)DataContext;

        public AzureKinectBodyTrackerVisualizerInstanceControl() {
            InitializeComponent();
        }

        private void UserControl_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e) {
            if (e.OldValue is AzureKinectBodyTrackerVisualizer old) {
                CompositionTarget.Rendering -= old.RenderingCallback;
            }
            if (e.NewValue is AzureKinectBodyTrackerVisualizer @new) {
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
