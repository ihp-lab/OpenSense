using System.Composition;
using System.Windows;
using OpenSense.WPF.Widget.Contract;

namespace OpenSense.WPF.Widget.DisplayPoiEstimatorBuilder {
    [Export(typeof(IWidgetMetadata))]
    public class WidgetMetadata : IWidgetMetadata {
        public string Name => "Display POI Estimator Builder";

        public string Description => "Wizard for creating Display POI Estimators.";

        public Window Create() => new CalibratorWindow();
    }
}
