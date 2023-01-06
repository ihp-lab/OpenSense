using System.Composition;
using System.Windows;
using OpenSense.WPF.Component.Contract;

namespace OpenSense.WPF.Component.OpenPose {
    [Export(typeof(IInstanceControlCreator))]
    public class OpenPoseInstanceControlCreator : IInstanceControlCreator {

        public bool CanCreate(object instance) => instance is OpenSense.Component.OpenPose.OpenPose;

        public UIElement Create(object instance) => new OpenPoseInstanceControl() { DataContext = instance };
    }
}
