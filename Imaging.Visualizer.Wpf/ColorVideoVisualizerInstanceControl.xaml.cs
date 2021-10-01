using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using OpenSense.Component.Imaging.Visualizer;

namespace OpenSense.Wpf.Component.Imaging.Visualizer {
    public partial class ColorVideoVisualizerInstanceControl : UserControl {

        private ColorVideoVisualizer ViewModel => (ColorVideoVisualizer)DataContext;

        public ColorVideoVisualizerInstanceControl() {
            InitializeComponent();
        }

        private void UserControl_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e) {
            if (e.OldValue is ColorVideoVisualizer old) {
                CompositionTarget.Rendering -= old.RenderingCallback;
            }
            if (e.NewValue is ColorVideoVisualizer @new) {
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
