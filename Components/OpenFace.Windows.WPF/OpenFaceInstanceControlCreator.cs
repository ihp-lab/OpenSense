using System.Composition;
using System.Windows;
using OpenSense.WPF.Components;

namespace OpenSense.WPF.Components.OpenFace {
    [Export(typeof(IInstanceControlCreator))]
    public class OpenFaceInstanceControlCreator : IInstanceControlCreator {

        public bool CanCreate(object instance) => instance is OpenSense.Components.OpenFace.OpenFace;

        public UIElement Create(object instance) => new OpenFaceInstanceControl() { DataContext = instance };
    }
}
