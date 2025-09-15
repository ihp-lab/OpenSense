#nullable enable

using System.Composition;
using System.Windows;
using OpenSense.Components;
using OpenSense.Components.FFMpeg;

namespace OpenSense.WPF.Components.FFMpeg {
    [Export(typeof(IConfigurationControlCreator))]
    public sealed class FileSourceConfigurationControlCreator : IConfigurationControlCreator {

        public bool CanCreate(ComponentConfiguration configuration) => configuration is FileSourceConfiguration;

        public UIElement Create(ComponentConfiguration configuration) => new FileSourceConfigurationControl() {
            DataContext = configuration
        };
    }
}
