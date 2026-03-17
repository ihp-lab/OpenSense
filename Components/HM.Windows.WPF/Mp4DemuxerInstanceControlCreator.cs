using System.Composition;
using System.Windows;
using OpenSense.Components.HM;

namespace OpenSense.WPF.Components.HM {
    [Export(typeof(IInstanceControlCreator))]
    public sealed class Mp4DemuxerInstanceControlCreator : IInstanceControlCreator {

        public bool CanCreate(object instance) => instance is Mp4Demuxer;

        public UIElement Create(object instance) => new Mp4DemuxerInstanceControl() {
            DataContext = instance,
        };
    }
}
