using System.Composition;
using System.Windows;
using OpenSense.Component.EyePointOfInterest;
using OpenSense.Wpf.Component.Contract;

namespace OpenSense.Wpf.Component.EyePointOfInterest {
    [Export(typeof(IInstanceControlCreator))]
    public class DisplayPoiEstimatorInstanceControlCreator : IInstanceControlCreator {

        public bool CanCreate(object instance) => instance is DisplayPoiEstimator;

        public UIElement Create(object instance) => new DisplayPoiEstimatorInstanceControl() { DataContext = instance };
    }
}
