using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using OpenSense.Components.Audio.Visualizer;

namespace OpenSense.WPF.Components.Audio.Visualizer {
    public partial class AudioVisualizerInstanceControl : UserControl {

        private AudioVisualizer ViewModel => (AudioVisualizer)DataContext;

        public AudioVisualizerInstanceControl() {
            InitializeComponent();
        }

        private void UserControl_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e) {
            if (e.OldValue is AudioVisualizer old) {
                CompositionTarget.Rendering -= old.RenderingCallback;
            }
            if (e.NewValue is AudioVisualizer @new) {
                CompositionTarget.Rendering += @new.RenderingCallback;
            }
            UpdateImageSize();
        }

        private void UserControl_Unloaded(object sender, RoutedEventArgs e) {
            if (ViewModel != null) {
                CompositionTarget.Rendering -= ViewModel.RenderingCallback;
            }
        }

        private void ImageCanvas_Changed(object sender, RoutedEventArgs e) {
            UpdateImageSize();
            e.Handled = true;
        }

        private void UpdateImageSize() {
            if (ImageCanvas.ActualWidth == 0 || ImageCanvas.ActualHeight == 0) {
                //To bypass false invokes, this kind of invokation happens every time after the image width and height are set.
                return;
            }
            if (ViewModel is null) {
                return;
            }
            var dpi = VisualTreeHelper.GetDpi(ImageCanvas);
            var width = ImageCanvas.ActualWidth / 96 * dpi.PixelsPerInchX;
            var height = ImageCanvas.ActualHeight / 96 * dpi.PixelsPerInchY;
            ViewModel.ImageWidth = Math.Max(1, (int)width);
            ViewModel.ImageHeight = Math.Max(1, (int)height);
        }
    }
}
