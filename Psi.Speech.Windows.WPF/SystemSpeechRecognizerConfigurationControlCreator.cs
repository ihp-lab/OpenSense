using System.Composition;
using System.Windows;
using OpenSense.Component.Contract;
using OpenSense.Component.Psi.Speech;
using OpenSense.WPF.Component.Contract;

namespace OpenSense.WPF.Component.Psi.Speech {
    [Export(typeof(IConfigurationControlCreator))]
    public class SystemSpeechRecognizerConfigurationControlCreator : IConfigurationControlCreator {

        public bool CanCreate(ComponentConfiguration configuration) => configuration is SystemSpeechRecognizerConfiguration;

        public UIElement Create(ComponentConfiguration configuration) => new SystemSpeechRecognizerConfigurationControl() { DataContext = configuration };
    }
}
