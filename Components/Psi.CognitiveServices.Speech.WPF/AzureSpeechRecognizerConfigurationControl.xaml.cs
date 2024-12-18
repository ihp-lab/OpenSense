﻿using System.Windows;
using System.Windows.Controls;
using OpenSense.Components.Psi.CognitiveServices.Speech;
using OpenSense.WPF.Components.Psi.Audio;

namespace OpenSense.WPF.Components.Psi.CognitiveServices.Speech {
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
