using System.Composition;
using System.Windows;
using OpenSense.Components;
using OpenSense.Components.Psi.Audio;

namespace OpenSense.WPF.Components.Psi.Audio {
    [Export(typeof(IConfigurationControlCreator))]
    public class AudioResamplerConfigurationControlCreator : IConfigurationControlCreator {

        public bool CanCreate(ComponentConfiguration configuration) => configuration is AudioResamplerConfiguration;

        public UIElement Create(ComponentConfiguration configuration) => new AudioResamplerConfigurationControl() { DataContext = configuration };
    }
}
