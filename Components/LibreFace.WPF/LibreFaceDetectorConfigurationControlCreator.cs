#nullable enable

using System.Composition;
using System.Windows;
using OpenSense.Components;
using OpenSense.Components.LibreFace;

namespace OpenSense.WPF.Components.LibreFace {
    [Export(typeof(IConfigurationControlCreator))]
    public sealed class LibreFaceDetectorConfigurationControlCreator : IConfigurationControlCreator {

        public bool CanCreate(ComponentConfiguration configuration) => configuration is LibreFaceDetectorConfiguration;

        public UIElement Create(ComponentConfiguration configuration) => new LibreFaceDetectorConfigurationControl() {
            DataContext = configuration
        };
    }
}
