using System.Composition;
using System.Windows;
using OpenSense.Components;
using OpenSense.Components.Psi.Audio;

namespace OpenSense.WPF.Components.Psi.Audio {
    [Export(typeof(IConfigurationControlCreator))]
    public class AudioPlayerConfigurationControlCreator : IConfigurationControlCreator {

        public bool CanCreate(ComponentConfiguration configuration) => configuration is AudioPlayerConfiguration;

        public UIElement Create(ComponentConfiguration configuration) => new AudioPlayerConfigurationControl() { DataContext = configuration };
    }
}
