using System.Composition;
using System.Windows;
using OpenSense.Components.Contract;
using OpenSense.Components.Psi.Media;
using OpenSense.WPF.Components.Contract;

namespace OpenSense.WPF.Components.Psi.Media {
    [Export(typeof(IConfigurationControlCreator))]
    public class MediaCaptureConfigurationControlCreator : IConfigurationControlCreator {

        public bool CanCreate(ComponentConfiguration configuration) => configuration is MediaCaptureConfiguration;

        public UIElement Create(ComponentConfiguration configuration) => new MediaCaptureConfigurationControl() { DataContext = configuration };
    }
}
