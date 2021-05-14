using System.Composition;
using System.Windows;
using OpenSense.Wpf.Widget.Contract;

namespace OpenSense.Wpf.Widget.DisplayPoiEstimatorBuilder {
    [Export(typeof(IWidgetMetadata))]
    public class WidgetMetadata : IWidgetMetadata {
        public string Name => "Display POI Estimator Builder";

        public string Description => "Wizard for creating Display POI Estimators.";

        public Window Create() => new CalibratorWindow();
    }
}
