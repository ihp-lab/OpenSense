using System.Windows;
using System.Windows.Controls;
using OpenSense.Components.Psi.Audio;

namespace OpenSense.WPF.Components.Psi.Audio {
    public sealed partial class AudioResamplerConfigurationControl : UserControl {

        private AudioResamplerConfiguration Configuration => (AudioResamplerConfiguration)DataContext;

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
            if (Configuration != null) {
                var control = new WaveFormatControl(Configuration.Raw.OutputFormat);
                ContentControlOutputFormat.Children.Add(control);
            }
        }
    }
}
