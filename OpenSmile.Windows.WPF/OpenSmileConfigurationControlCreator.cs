using System.Composition;
using System.Windows;
using OpenSense.Components.Contract;
using OpenSense.Components.OpenSmile;
using OpenSense.WPF.Components.Contract;

namespace OpenSense.WPF.Components.OpenSmile {
    [Export(typeof(IConfigurationControlCreator))]
    public class OpenSmileConfigurationControlCreator : IConfigurationControlCreator {

        public bool CanCreate(ComponentConfiguration configuration) => configuration is OpenSmileConfiguration;

        public UIElement Create(ComponentConfiguration configuration) => new OpenSmileConfigurationControl() { DataContext = configuration };
    }
}
