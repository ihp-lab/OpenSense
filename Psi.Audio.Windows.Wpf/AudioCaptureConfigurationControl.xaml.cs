using System.Linq;
using System.Windows;
using System.Windows.Controls;
using OpenSense.Component.Psi.Audio;

namespace OpenSense.Wpf.Component.Psi.Audio {
    public partial class AudioCaptureConfigurationControl : UserControl {

        private AudioCaptureConfiguration Config => DataContext as AudioCaptureConfiguration;

        public AudioCaptureConfigurationControl() {
            InitializeComponent();
        }

        private void ComboBoxMicrophone_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e) {
            if (Config != null) {
                var devs = AudioDevice.Devices().ToList();
                ComboBoxMicrophone.ItemsSource = devs;
                ComboBoxMicrophone.DisplayMemberPath = nameof(AudioDevice.Name);
                var index = devs.FindIndex(dev => dev.Name == Config.Raw.DeviceName);
                ComboBoxMicrophone.SelectedIndex = index >= 0 ? index : 0;
            }
        }

        private void ComboBoxMicrophone_SelectionChanged(object sender, SelectionChangedEventArgs e) {
            if (ComboBoxMicrophone.SelectedItem is AudioDevice dev) {
                Config.Raw.DeviceName = dev.Name;
            }
        }

        private void ContentControlWaveFormat_Loaded(object sender, RoutedEventArgs e) {
            ContentControlWaveFormat.Children.Clear();
            if (Config != null) {
                if (Config.Raw.Format is null) {
                    Config.Raw.Format = Microsoft.Psi.Audio.WaveFormat.Create16kHz1Channel16BitPcm();
                }
                var control = new WaveFormatControl(Config.Raw.Format);
                ContentControlWaveFormat.Children.Add(control);
            }
        }

        
    }
}
