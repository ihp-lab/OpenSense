using System.Composition;
using System.Windows;
using OpenSense.Components.Contract;
using OpenSense.Components.Psi.Audio;
using OpenSense.WPF.Components.Contract;

namespace OpenSense.WPF.Components.Psi.Audio {
    [Export(typeof(IConfigurationControlCreator))]
    public class AudioPlayerConfigurationControlCreator : IConfigurationControlCreator {

        public bool CanCreate(ComponentConfiguration configuration) => configuration is AudioPlayerConfiguration;

        public UIElement Create(ComponentConfiguration configuration) => new AudioPlayerConfigurationControl() { DataContext = configuration };
    }
}
