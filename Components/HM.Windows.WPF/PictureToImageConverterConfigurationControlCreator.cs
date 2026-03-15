#nullable enable

using System.Composition;
using System.Windows;
using OpenSense.Components;
using OpenSense.Components.HM;

namespace OpenSense.WPF.Components.HM {
    [Export(typeof(IConfigurationControlCreator))]
    public sealed class PictureToImageConverterConfigurationControlCreator : IConfigurationControlCreator {

        public bool CanCreate(ComponentConfiguration configuration) => configuration is PictureToImageConverterConfiguration;

        public UIElement Create(ComponentConfiguration configuration) => new PictureToImageConverterConfigurationControl() {
            DataContext = configuration
        };
    }
}
