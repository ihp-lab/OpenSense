using System.Composition;
using System.Windows;
using OpenSense.Component.BodyGestureDetectors;
using OpenSense.Wpf.Component.Contract;

namespace OpenSense.Wpf.Component.BodyGestureDetectors {
    [Export(typeof(IInstanceControlCreator))]
    public class BodySwirlDetectorInstanceControlCreator : IInstanceControlCreator {

        public bool CanCreate(object instance) => instance is BodySwirlDetector;

        public UIElement Create(object instance) => new BodySwirlDetectorInstanceControl() { DataContext = instance };
    }
}
