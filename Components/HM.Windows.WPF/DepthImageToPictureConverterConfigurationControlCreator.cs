#nullable enable

using System.Composition;
using System.Windows;
using OpenSense.Components;
using OpenSense.Components.HM;

namespace OpenSense.WPF.Components.HM {
    [Export(typeof(IConfigurationControlCreator))]
    public sealed class DepthImageToPictureConverterConfigurationControlCreator : IConfigurationControlCreator {

        public bool CanCreate(ComponentConfiguration configuration) => configuration is DepthImageToPictureConverterConfiguration;

        public UIElement Create(ComponentConfiguration configuration) => new DepthImageToPictureConverterConfigurationControl() {
            DataContext = configuration
        };
    }
}
