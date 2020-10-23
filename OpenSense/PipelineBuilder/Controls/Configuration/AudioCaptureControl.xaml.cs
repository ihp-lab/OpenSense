using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using OpenSense.PipelineBuilder.Configurations;
using OpenSense.Utilities;

namespace OpenSense.PipelineBuilder.Controls.Configuration {
    public partial class AudioCaptureControl : UserControl {

        private AudioCaptureConfiguration Config => DataContext as AudioCaptureConfiguration;

        public AudioCaptureControl() {
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
                if (Config.Raw.OutputFormat is null) {
                    Config.Raw.OutputFormat = Microsoft.Psi.Audio.WaveFormat.Create16kHz1Channel16BitPcm();
                }
                var control = new WaveFormatControl(Config.Raw.OutputFormat);
                ContentControlWaveFormat.Children.Add(control);
            }
        }

        
    }
}
