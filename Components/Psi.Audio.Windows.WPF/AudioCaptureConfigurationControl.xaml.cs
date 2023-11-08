using System.Linq;
using System.Windows;
using System.Windows.Controls;
using OpenSense.Components.Psi.Audio;

namespace OpenSense.WPF.Components.Psi.Audio {
    public partial class AudioCaptureConfigurationControl : UserControl {

        private AudioCaptureConfiguration Configuration => (AudioCaptureConfiguration)DataContext;

        public AudioCaptureConfigurationControl() {
            InitializeComponent();
        }

        private void ComboBoxMicrophone_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e) {
            if (Configuration != null) {
                var devs = AudioDevice.Devices().ToList();
                ComboBoxMicrophone.ItemsSource = devs;
                ComboBoxMicrophone.DisplayMemberPath = nameof(AudioDevice.Name);
                var index = devs.FindIndex(dev => dev.Name == Configuration.Raw.DeviceName);
                ComboBoxMicrophone.SelectedIndex = index >= 0 ? index : 0;
            }
        }

        private void ComboBoxMicrophone_SelectionChanged(object sender, SelectionChangedEventArgs e) {
            if (ComboBoxMicrophone.SelectedItem is AudioDevice dev) {
                Configuration.Raw.DeviceName = dev.Name;
            }
        }

        private void ContentControlWaveFormat_Loaded(object sender, RoutedEventArgs e) {
            ContentControlWaveFormat.Children.Clear();
            if (Configuration != null) {
                if (Configuration.Raw.Format is null) {
                    Configuration.Raw.Format = Microsoft.Psi.Audio.WaveFormat.Create16kHz1Channel16BitPcm();
                }
                var control = new WaveFormatControl(Configuration.Raw.Format);
                ContentControlWaveFormat.Children.Add(control);
            }
        }

        
    }
}
