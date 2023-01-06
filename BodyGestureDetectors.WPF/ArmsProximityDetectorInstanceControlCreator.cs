using System.Composition;
using System.Windows;
using OpenSense.Components.BodyGestureDetectors;
using OpenSense.WPF.Components.Contract;

namespace OpenSense.WPF.Components.BodyGestureDetectors {
    [Export(typeof(IInstanceControlCreator))]
    public class ArmsProximityDetectorInstanceControlCreator : IInstanceControlCreator {

        public bool CanCreate(object instance) => instance is ArmsProximityDetector;

        public UIElement Create(object instance) => new ArmsProximityDetectorInstanceControl() { DataContext = instance };
    }
}
