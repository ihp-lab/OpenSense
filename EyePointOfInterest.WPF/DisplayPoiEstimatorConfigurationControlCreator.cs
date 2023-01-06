using System.Composition;
using System.Windows;
using OpenSense.Component.Contract;
using OpenSense.Component.EyePointOfInterest;
using OpenSense.WPF.Component.Contract;

namespace OpenSense.WPF.Component.EyePointOfInterest {
    [Export(typeof(IConfigurationControlCreator))]
    public class DisplayPoiEstimatorConfigurationControlCreator : IConfigurationControlCreator {

        public bool CanCreate(ComponentConfiguration configuration) => configuration is DisplayPoiEstimatorConfiguration;

        public UIElement Create(ComponentConfiguration configuration) => new DisplayPoiEstimatorConfigurationControl() { DataContext = configuration };
    }
}
