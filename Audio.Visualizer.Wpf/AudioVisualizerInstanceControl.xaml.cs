using System.Windows.Controls;
using System.Windows.Media;
using OpenSense.Component.Audio.Visualizer;

namespace OpenSense.Wpf.Component.Audio.Visualizer {
    public partial class AudioVisualizerInstanceControl : UserControl {

        private AudioVisualizer ViewModel => (AudioVisualizer)DataContext;

        public AudioVisualizerInstanceControl() {
            InitializeComponent();
        }

        private void UserControl_DataContextChanged(object sender, System.Windows.DependencyPropertyChangedEventArgs e) {
            if (e.OldValue is AudioVisualizer old) {
                CompositionTarget.Rendering -= old.RenderingCallback;
            }
            if (e.NewValue is AudioVisualizer @new) {
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
