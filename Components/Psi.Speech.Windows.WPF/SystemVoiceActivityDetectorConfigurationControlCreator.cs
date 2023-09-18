using System.Composition;
using System.Windows;
using OpenSense.Components;
using OpenSense.Components.Psi.Speech;
using OpenSense.WPF.Components;

namespace OpenSense.WPF.Components.Psi.Speech {
    [Export(typeof(IConfigurationControlCreator))]
    public class SystemVoiceActivityDetectorConfigurationControlCreator : IConfigurationControlCreator {

        public bool CanCreate(ComponentConfiguration configuration) => configuration is SystemVoiceActivityDetectorConfiguration;

        public UIElement Create(ComponentConfiguration configuration) => new SystemVoiceActivityDetectorConfigurationControl() { DataContext = configuration };
    }
}
