using System.Composition;
using System.Windows;

namespace OpenSense.WPF.Widgets.DisplayPoiEstimatorBuilder {
    [Export(typeof(IWidgetMetadata))]
    public sealed class WidgetMetadata : IWidgetMetadata {
        public string Name => "Display POI Estimator Builder";

        public string Description => "Wizard for creating Display POI Estimators.";

        public Window Create() => new CalibratorWindow();
    }
}
