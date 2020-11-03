using System.Windows;
using System.Windows.Controls;
using OpenSense.Component.Psi.Audio;

namespace OpenSense.Wpf.Component.Psi.Audio {
    public partial class AudioResamplerConfigurationControl : UserControl {

        private AudioResamplerConfiguration Config => DataContext as AudioResamplerConfiguration;

        public AudioResamplerConfigurationControl() {
            InitializeComponent();
        }
        /*
        private void ContentControlInputFormat_Loaded(object sender, RoutedEventArgs e) {
            ContentControlInputFormat.Children.Clear();
            if (Config != null) {
                var control = new WaveFormatControl(Config.Raw.InputFormat);
                ContentControlInputFormat.Children.Add(control);
            }
        }
        */
        private void ContentControlOutputFormat_Loaded(object sender, RoutedEventArgs e) {
            ContentControlOutputFormat.Children.Clear();
            if (Config != null) {
                var control = new WaveFormatControl(Config.Raw.OutputFormat);
                ContentControlOutputFormat.Children.Add(control);
            }
        }
    }
}
