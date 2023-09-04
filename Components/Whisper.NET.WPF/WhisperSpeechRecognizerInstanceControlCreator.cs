using System.Composition;
using System.Windows;
using OpenSense.Components.Whisper.NET;
using OpenSense.WPF.Components.Contract;
namespace OpenSense.WPF.Components.Whisper.NET {
    [Export(typeof(IInstanceControlCreator))]
    public sealed class WhisperSpeechRecognizerInstanceControlCreator : IInstanceControlCreator {

        public bool CanCreate(object instance) => instance is WhisperSpeechRecognizer;

        public UIElement Create(object instance) => new WhisperSpeechRecognizerInstanceControl() { 
            DataContext = instance 
        };
    }
}
