using System.Composition;
using System.Windows;
using OpenSense.Component.Contract;
using OpenSense.Component.Psi.Speech;
using OpenSense.Wpf.Component.Contract;

namespace OpenSense.Wpf.Component.Psi.Speech {
    [Export(typeof(IConfigurationControlCreator))]
    public class SystemVoiceActivityDetectorConfigurationControlCreator : IConfigurationControlCreator {

        public bool CanCreate(ComponentConfiguration configuration) => configuration is SystemVoiceActivityDetectorConfiguration;

        public UIElement Create(ComponentConfiguration configuration) => new SystemVoiceActivityDetectorConfigurationControl() { DataContext = configuration };
    }
}
