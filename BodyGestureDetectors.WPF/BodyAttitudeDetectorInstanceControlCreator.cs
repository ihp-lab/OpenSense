using System.Composition;
using System.Windows;
using OpenSense.Component.BodyGestureDetectors;
using OpenSense.WPF.Component.Contract;

namespace OpenSense.WPF.Component.BodyGestureDetectors {
    [Export(typeof(IInstanceControlCreator))]
    public class BodyAttitudeDetectorInstanceControlCreator : IInstanceControlCreator {

        public bool CanCreate(object instance) => instance is BodyAttitudeDetector;

        public UIElement Create(object instance) => new BodyAttitudeDetectorInstanceControl() { DataContext = instance };
    }
}
