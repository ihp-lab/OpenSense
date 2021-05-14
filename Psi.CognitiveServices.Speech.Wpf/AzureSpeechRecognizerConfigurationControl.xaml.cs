using System;
using System.Windows;
using System.Windows.Controls;
using OpenSense.Component.Psi.CognitiveServices.Speech;
using OpenSense.Wpf.Component.Psi.Audio.Common;

namespace OpenSense.Wpf.Component.Psi.CognitiveServices.Speech {
    public partial class AzureSpeechRecognizerConfigurationControl : UserControl {

        private AzureSpeechRecognizerConfiguration Config => DataContext as AzureSpeechRecognizerConfiguration;

        public AzureSpeechRecognizerConfigurationControl() {
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
