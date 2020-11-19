using System.Composition;
using System.Windows;
using OpenSense.Component.Contract;
using OpenSense.Component.Psi.Media;
using OpenSense.Wpf.Component.Contract;

namespace OpenSense.Wpf.Component.Psi.Media {
    [Export(typeof(IConfigurationControlCreator))]
    public class MediaCaptureConfigurationControlCreator : IConfigurationControlCreator {

        public bool CanCreate(ComponentConfiguration configuration) => configuration is MediaCaptureConfiguration;

        public UIElement Create(ComponentConfiguration configuration) => new MediaCaptureConfigurationControl() { DataContext = configuration };
    }
}
