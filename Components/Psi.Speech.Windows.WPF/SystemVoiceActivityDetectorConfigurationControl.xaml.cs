using System.Windows;
using System.Windows.Controls;
using OpenSense.Components.Psi.Speech;
using OpenSense.WPF.Components.Psi.Audio.Common;

namespace OpenSense.WPF.Components.Psi.Speech {
    public partial class SystemVoiceActivityDetectorConfigurationControl : UserControl {

        private SystemVoiceActivityDetectorConfiguration Config => DataContext as SystemVoiceActivityDetectorConfiguration;

        public SystemVoiceActivityDetectorConfigurationControl() {
            InitializeComponent();
        }

        private void ContentControlInputFormat_Loaded(object sender, RoutedEventArgs e) {
            ContentControlInputFormat.Children.Clear();
            if (Config != null) {
                var control = new WaveFormatControl(Config.Raw.InputFormat);
                ContentControlInputFormat.Children.Add(control);
            }
        }
    }
}
