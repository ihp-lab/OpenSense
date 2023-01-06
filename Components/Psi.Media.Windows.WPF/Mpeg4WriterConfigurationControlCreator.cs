using System.Composition;
using System.Windows;
using OpenSense.Components.Contract;
using OpenSense.Components.Psi.Media;
using OpenSense.WPF.Components.Contract;

namespace OpenSense.WPF.Components.Psi.Media {
    [Export(typeof(IConfigurationControlCreator))]
    public class Mpeg4WriterConfigurationControlCreator : IConfigurationControlCreator {

        public bool CanCreate(ComponentConfiguration configuration) => configuration is Mpeg4WriterConfiguration;

        public UIElement Create(ComponentConfiguration configuration) => new Mpeg4WriterConfigurationControl() { DataContext = configuration };
    }
}
