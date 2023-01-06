using System.Composition;
using System.Windows;
using OpenSense.Component.Contract;
using OpenSense.Component.Psi.Media;
using OpenSense.WPF.Component.Contract;

namespace OpenSense.WPF.Component.Psi.Media {
    [Export(typeof(IConfigurationControlCreator))]
    public class Mpeg4WriterConfigurationControlCreator : IConfigurationControlCreator {

        public bool CanCreate(ComponentConfiguration configuration) => configuration is Mpeg4WriterConfiguration;

        public UIElement Create(ComponentConfiguration configuration) => new Mpeg4WriterConfigurationControl() { DataContext = configuration };
    }
}
