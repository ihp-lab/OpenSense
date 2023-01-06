using System.Composition;
using System.Windows;
using OpenSense.Components.EyePointOfInterest;
using OpenSense.WPF.Components.Contract;

namespace OpenSense.WPF.Components.EyePointOfInterest {
    [Export(typeof(IInstanceControlCreator))]
    public class DisplayPoiEstimatorInstanceControlCreator : IInstanceControlCreator {

        public bool CanCreate(object instance) => instance is DisplayPoiEstimator;

        public UIElement Create(object instance) => new DisplayPoiEstimatorInstanceControl() { DataContext = instance };
    }
}
