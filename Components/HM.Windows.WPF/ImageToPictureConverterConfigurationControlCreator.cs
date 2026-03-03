#nullable enable

using System.Composition;
using System.Windows;
using OpenSense.Components;
using OpenSense.Components.HM;

namespace OpenSense.WPF.Components.HM {
    [Export(typeof(IConfigurationControlCreator))]
    public sealed class ImageToPictureConverterConfigurationControlCreator : IConfigurationControlCreator {

        public bool CanCreate(ComponentConfiguration configuration) => configuration is ImageToPictureConverterConfiguration;

        public UIElement Create(ComponentConfiguration configuration) => new ImageToPictureConverterConfigurationControl() {
            DataContext = configuration
        };
    }
}
