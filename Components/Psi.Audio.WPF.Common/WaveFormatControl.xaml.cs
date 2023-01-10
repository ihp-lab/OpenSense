using System.Windows;
using System.Windows.Controls;
using Microsoft.Psi.Audio;

namespace OpenSense.WPF.Components.Psi.Audio {
    public partial class WaveFormatControl : UserControl {

        private WaveFormat Format;

        public WaveFormatControl(WaveFormat waveFormat) {
            InitializeComponent();
            Format = waveFormat;
            DataContext = waveFormat;
        }

        private void Update(WaveFormat format) {
            Format.CopyFrom(format);
            DataContext = null;
            DataContext = Format;
        }

        private void ButtonCreate16kHz1Channel16BitPcm_Click(object sender, RoutedEventArgs e) {
            Update(WaveFormat.Create16kHz1Channel16BitPcm());
        }

        private void ButtonCreate16BitPcm_Click(object sender, RoutedEventArgs e) {
            Update(WaveFormat.Create16BitPcm((int)Format.SamplesPerSec, Format.Channels));
        }

        private void ButtonCreatePcm_Click(object sender, RoutedEventArgs e) {
            Update(WaveFormat.CreatePcm((int)Format.SamplesPerSec, Format.BitsPerSample, Format.Channels));
        }

        private void ButtonCreate16kHz1ChannelIeeeFloat_Click(object sender, RoutedEventArgs e) {
            Update(WaveFormat.Create16kHz1ChannelIeeeFloat());
        }

        private void ButtonCreateIeeeFloat_Click(object sender, RoutedEventArgs e) {
            Update(WaveFormat.CreateIeeeFloat((int)Format.SamplesPerSec, Format.Channels));
        }
    }
}
