using System.Composition;
using System.Windows;
using OpenSense.Components.Contract;
using OpenSense.Components.Psi.Speech;
using OpenSense.WPF.Components.Contract;

namespace OpenSense.WPF.Components.Psi.Speech {
    [Export(typeof(IConfigurationControlCreator))]
    public class SystemSpeechRecognizerConfigurationControlCreator : IConfigurationControlCreator {

        public bool CanCreate(ComponentConfiguration configuration) => configuration is SystemSpeechRecognizerConfiguration;

        public UIElement Create(ComponentConfiguration configuration) => new SystemSpeechRecognizerConfigurationControl() { DataContext = configuration };
    }
}
