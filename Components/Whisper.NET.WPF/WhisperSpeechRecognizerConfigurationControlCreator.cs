using System.Composition;
using System.Windows;
using OpenSense.Components;
using OpenSense.Components.Whisper.NET;
using OpenSense.WPF.Components;

namespace OpenSense.WPF.Components.Whisper.NET {
    [Export(typeof(IConfigurationControlCreator))]
    public sealed class WhisperSpeechRecognizerConfigurationControlCreator : IConfigurationControlCreator {

        public bool CanCreate(ComponentConfiguration configuration) => configuration is WhisperSpeechRecognizerConfiguration;

        public UIElement Create(ComponentConfiguration configuration) => new WhisperSpeechRecognizerConfigurationControl() { 
            DataContext = configuration 
        };
    }
}
