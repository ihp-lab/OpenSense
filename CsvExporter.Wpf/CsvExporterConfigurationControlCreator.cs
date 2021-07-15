using System.Composition;
using System.Windows;
using OpenSense.Component.Contract;
using OpenSense.Component.CsvExporter;
using OpenSense.Wpf.Component.Contract;

namespace OpenSense.Wpf.Component.CsvExporter {
    [Export(typeof(IConfigurationControlCreator))]
    public class CsvExporterConfigurationControlCreator : IConfigurationControlCreator {

        public bool CanCreate(ComponentConfiguration configuration) => configuration is CsvExporterConfiguration;

        public UIElement Create(ComponentConfiguration configuration) => new CsvExporterConfigurationControl() { DataContext = configuration };
    }
}
