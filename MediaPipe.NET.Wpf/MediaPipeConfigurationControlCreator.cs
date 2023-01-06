using System.Composition;
using System.Windows;
using OpenSense.Component.Contract;
using OpenSense.Component.MediaPipe.NET;
using OpenSense.Wpf.Component.Contract;

namespace OpenSense.Wpf.Component.MediaPipe.NET {
    [Export(typeof(IConfigurationControlCreator))]
    public sealed class MediaPipeConfigurationControlCreator : IConfigurationControlCreator {

        public bool CanCreate(ComponentConfiguration configuration) => configuration is MediaPipeConfiguration;

        public UIElement Create(ComponentConfiguration configuration) => new MediaPipeConfigurationControl() { DataContext = configuration };
    }
}
