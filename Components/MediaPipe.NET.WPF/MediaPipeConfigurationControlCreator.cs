using System.Composition;
using System.Windows;
using OpenSense.Components.Contract;
using OpenSense.Components.MediaPipe.NET;
using OpenSense.WPF.Components.Contract;

namespace OpenSense.WPF.Components.MediaPipe.NET {
    [Export(typeof(IConfigurationControlCreator))]
    public sealed class MediaPipeConfigurationControlCreator : IConfigurationControlCreator {

        public bool CanCreate(ComponentConfiguration configuration) => configuration is MediaPipeConfiguration;

        public UIElement Create(ComponentConfiguration configuration) => new MediaPipeConfigurationControl() { DataContext = configuration };
    }
}
