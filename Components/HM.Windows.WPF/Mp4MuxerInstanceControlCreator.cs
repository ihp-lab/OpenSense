using System.Composition;
using System.Windows;
using OpenSense.Components.HM;

namespace OpenSense.WPF.Components.HM {
    [Export(typeof(IInstanceControlCreator))]
    public sealed class Mp4MuxerInstanceControlCreator : IInstanceControlCreator {

        public bool CanCreate(object instance) => instance is Mp4Muxer;

        public UIElement Create(object instance) => new Mp4MuxerInstanceControl() {
            DataContext = instance,
        };
    }
}
