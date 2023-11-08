using System.Windows.Controls;
using OpenSense.Components.Psi.Audio;

namespace OpenSense.WPF.Components.Psi.Audio {
    public partial class AudioPlayerConfigurationControl : UserControl {

        private AudioPlayerConfiguration Configuration => (AudioPlayerConfiguration)DataContext;

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
