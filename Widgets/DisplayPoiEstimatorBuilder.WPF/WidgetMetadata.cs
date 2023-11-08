using System.Composition;
using System.Windows;
using OpenSense.WPF.Widgets;

namespace OpenSense.WPF.Widgets.DisplayPoiEstimatorBuilder {
    [Export(typeof(IWidgetMetadata))]
    public class WidgetMetadata : IWidgetMetadata {
        public string Name => "Display POI Estimator Builder";

        public string Description => "Wizard for creating Display POI Estimators.";

        public Window Create() => new CalibratorWindow();
    }
}
