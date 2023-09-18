using System.Composition;
using System.Windows;
using OpenSense.Components;
using OpenSense.Components.CsvExporter;

namespace OpenSense.WPF.Components.CsvExporter {
    [Export(typeof(IConfigurationControlCreator))]
    public class CsvExporterConfigurationControlCreator : IConfigurationControlCreator {

        public bool CanCreate(ComponentConfiguration configuration) => configuration is CsvExporterConfiguration;

        public UIElement Create(ComponentConfiguration configuration) => new CsvExporterConfigurationControl() { DataContext = configuration };
    }
}
