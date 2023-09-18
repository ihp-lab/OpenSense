using System.Composition;
using System.Windows;
using OpenSense.Components.BodyGestureDetectors;
using OpenSense.WPF.Components;

namespace OpenSense.WPF.Components.BodyGestureDetectors {
    [Export(typeof(IInstanceControlCreator))]
    public class BodyAttitudeDetectorInstanceControlCreator : IInstanceControlCreator {

        public bool CanCreate(object instance) => instance is BodyAttitudeDetector;

        public UIElement Create(object instance) => new BodyAttitudeDetectorInstanceControl() { DataContext = instance };
    }
}
