using System.Windows;
using System.Windows.Controls;
using OpenSense.Component.Psi.Speech;
using OpenSense.WPF.Component.Psi.Audio.Common;

namespace OpenSense.WPF.Component.Psi.Speech {
    public partial class SystemSpeechRecognizerConfigurationControl : UserControl {

        private SystemSpeechRecognizerConfiguration Config => DataContext as SystemSpeechRecognizerConfiguration;

        public SystemSpeechRecognizerConfigurationControl() {
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
