using System.Composition;
using System.Windows;

namespace OpenSense.WPF.Widgets.OpenSmileConfigurationConverter {
    [Export(typeof(IWidgetMetadata))]
    public class WidgetMetadata : IWidgetMetadata {
        public string Name => "OpenSmile Configuration Converter";

        public string Description => "Modify selected standard openSMILE configuration file taking usage of OpenSense.Components.OpenSmile built-in wave source and data sink components.";

        public Window Create() => new ConverterWindow();
    }
}
