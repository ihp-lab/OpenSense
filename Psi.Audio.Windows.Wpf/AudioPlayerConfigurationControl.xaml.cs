using System.Windows.Controls;
using OpenSense.Component.Psi.Audio;

namespace OpenSense.WPF.Component.Psi.Audio {
    public partial class AudioPlayerConfigurationControl : UserControl {

        private AudioPlayerConfiguration Config => DataContext as AudioPlayerConfiguration;

        public AudioPlayerConfigurationControl() {
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
    }
}
