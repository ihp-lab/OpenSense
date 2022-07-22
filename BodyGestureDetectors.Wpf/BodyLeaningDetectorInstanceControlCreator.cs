using System.Composition;
using System.Windows;
using OpenSense.Component.BodyGestureDetectors;
using OpenSense.Wpf.Component.Contract;

namespace OpenSense.Wpf.Component.BodyGestureDetectors {
    [Export(typeof(IInstanceControlCreator))]
    public class BodyLeaningDetectorInstanceControlCreator : IInstanceControlCreator {

        public bool CanCreate(object instance) => instance is BodyLeaningDetector;

        public UIElement Create(object instance) => new BodyLeaningDetectorInstanceControl() { DataContext = instance };
    }
}
