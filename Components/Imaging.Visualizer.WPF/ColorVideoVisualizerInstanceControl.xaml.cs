using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using OpenSense.Components.Imaging.Visualizer;

namespace OpenSense.WPF.Components.Imaging.Visualizer {
    public partial class ImageVisualizerInstanceControl : UserControl {

        private ImageVisualizer ViewModel => (ImageVisualizer)DataContext;

        public ImageVisualizerInstanceControl() {
            InitializeComponent();
        }

        private void UserControl_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e) {
            if (e.OldValue is ImageVisualizer old) {
                CompositionTarget.Rendering -= old.RenderingCallback;
            }
            if (e.NewValue is ImageVisualizer @new) {
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
