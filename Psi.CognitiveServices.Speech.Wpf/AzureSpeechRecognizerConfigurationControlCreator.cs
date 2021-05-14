using System.Composition;
using System.Windows;
using OpenSense.Component.Contract;
using OpenSense.Component.Psi.CognitiveServices.Speech;
using OpenSense.Wpf.Component.Contract;

namespace OpenSense.Wpf.Component.Psi.CognitiveServices.Speech {
    [Export(typeof(IConfigurationControlCreator))]
    public class AzureSpeechRecognizerConfigurationControlCreator : IConfigurationControlCreator {

        public bool CanCreate(ComponentConfiguration configuration) => configuration is AzureSpeechRecognizerConfiguration;

        public UIElement Create(ComponentConfiguration configuration) => new AzureSpeechRecognizerConfigurationControl() { DataContext = configuration };
    }
}
