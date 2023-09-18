using System.Composition;
using System.Windows;
using OpenSense.Components;
using OpenSense.Components.OpenSmile;

namespace OpenSense.WPF.Components.OpenSmile {
    [Export(typeof(IConfigurationControlCreator))]
    public class OpenSmileConfigurationControlCreator : IConfigurationControlCreator {

        public bool CanCreate(ComponentConfiguration configuration) => configuration is OpenSmileConfiguration;

        public UIElement Create(ComponentConfiguration configuration) => new OpenSmileConfigurationControl() { DataContext = configuration };
    }
}
