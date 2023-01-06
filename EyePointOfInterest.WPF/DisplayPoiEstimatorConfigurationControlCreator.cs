using System.Composition;
using System.Windows;
using OpenSense.Components.Contract;
using OpenSense.Components.EyePointOfInterest;
using OpenSense.WPF.Components.Contract;

namespace OpenSense.WPF.Components.EyePointOfInterest {
    [Export(typeof(IConfigurationControlCreator))]
    public class DisplayPoiEstimatorConfigurationControlCreator : IConfigurationControlCreator {

        public bool CanCreate(ComponentConfiguration configuration) => configuration is DisplayPoiEstimatorConfiguration;

        public UIElement Create(ComponentConfiguration configuration) => new DisplayPoiEstimatorConfigurationControl() { DataContext = configuration };
    }
}
