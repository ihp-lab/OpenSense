using System.Composition;
using System.Windows;
using OpenSense.Components;
using OpenSense.Components.Psi.Audio;
using OpenSense.WPF.Components;

namespace OpenSense.WPF.Components.Psi.Audio {
    [Export(typeof(IConfigurationControlCreator))]
    public class WaveFileWriterConfigurationControlCreator : IConfigurationControlCreator {

        public bool CanCreate(ComponentConfiguration configuration) => configuration is WaveFileWriterConfiguration;

        public UIElement Create(ComponentConfiguration configuration) => new WaveFileWriterConfigurationControl() { DataContext = configuration };
    }
}
