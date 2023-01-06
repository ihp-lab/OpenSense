using System.Composition;
using System.Windows;
using OpenSense.WPF.Components.Contract;

namespace OpenSense.WPF.Components.OpenPose {
    [Export(typeof(IInstanceControlCreator))]
    public class OpenPoseInstanceControlCreator : IInstanceControlCreator {

        public bool CanCreate(object instance) => instance is OpenSense.Components.OpenPose.OpenPose;

        public UIElement Create(object instance) => new OpenPoseInstanceControl() { DataContext = instance };
    }
}
