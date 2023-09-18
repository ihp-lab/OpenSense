using System.Composition;
using System.Windows;
using OpenSense.Components;
using OpenSense.Components.Psi.Media;

namespace OpenSense.WPF.Components.Psi.Media {
    [Export(typeof(IConfigurationControlCreator))]
    public class MediaSourceConfigurationControlCreator : IConfigurationControlCreator {

        public bool CanCreate(ComponentConfiguration configuration) => configuration is MediaSourceConfiguration;

        public UIElement Create(ComponentConfiguration configuration) => new MediaSourceConfigurationControl() { DataContext = configuration };
    }
}
