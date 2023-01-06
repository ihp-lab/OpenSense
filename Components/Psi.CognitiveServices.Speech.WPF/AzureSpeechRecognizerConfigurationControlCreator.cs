using System.Composition;
using System.Windows;
using OpenSense.Components.Contract;
using OpenSense.Components.Psi.CognitiveServices.Speech;
using OpenSense.WPF.Components.Contract;

namespace OpenSense.WPF.Components.Psi.CognitiveServices.Speech {
    [Export(typeof(IConfigurationControlCreator))]
    public class AzureSpeechRecognizerConfigurationControlCreator : IConfigurationControlCreator {

        public bool CanCreate(ComponentConfiguration configuration) => configuration is AzureSpeechRecognizerConfiguration;

        public UIElement Create(ComponentConfiguration configuration) => new AzureSpeechRecognizerConfigurationControl() { DataContext = configuration };
    }
}
