using System.Composition;
using System.Windows;
using OpenSense.Component.Contract;
using OpenSense.Component.Psi.Audio;
using OpenSense.Wpf.Component.Contract;

namespace OpenSense.Wpf.Component.Psi.Audio {
    [Export(typeof(IConfigurationControlCreator))]
    public class WaveFileWriterConfigurationControlCreator : IConfigurationControlCreator {

        public bool CanCreate(ComponentConfiguration configuration) => configuration is WaveFileWriterConfiguration;

        public UIElement Create(ComponentConfiguration configuration) => new WaveFileWriterConfigurationControl() { DataContext = configuration };
    }
}
