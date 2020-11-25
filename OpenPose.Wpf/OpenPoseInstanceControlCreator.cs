using System.Composition;
using System.Windows;
using OpenSense.Wpf.Component.Contract;

namespace OpenSense.Wpf.Component.OpenPose {
    [Export(typeof(IInstanceControlCreator))]
    public class OpenPoseInstanceControlCreator : IInstanceControlCreator {

        public bool CanCreate(object instance) => instance is OpenSense.Component.OpenPose.OpenPose;

        public UIElement Create(object instance) => new OpenPoseInstanceControl() { DataContext = instance };
    }
}
