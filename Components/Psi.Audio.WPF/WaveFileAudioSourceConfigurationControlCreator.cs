using System.Composition;
using System.Windows;
using OpenSense.Components.Contract;
using OpenSense.Components.Psi.Audio;
using OpenSense.WPF.Components.Contract;

namespace OpenSense.WPF.Components.Psi.Audio {
    [Export(typeof(IConfigurationControlCreator))]
    public class WaveFileAudioSourceConfigurationControlCreator : IConfigurationControlCreator {

        public bool CanCreate(ComponentConfiguration configuration) => configuration is WaveFileAudioSourceConfiguration;

        public UIElement Create(ComponentConfiguration configuration) => new WaveFileAudioSourceConfigurationControl() { DataContext = configuration };
    }
}
