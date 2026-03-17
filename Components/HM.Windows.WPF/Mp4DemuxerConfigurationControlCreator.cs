using System.Composition;
using System.Windows;
using OpenSense.Components;
using OpenSense.Components.HM;

namespace OpenSense.WPF.Components.HM {
    [Export(typeof(IConfigurationControlCreator))]
    public sealed class Mp4DemuxerConfigurationControlCreator : IConfigurationControlCreator {

        public bool CanCreate(ComponentConfiguration configuration) => configuration is Mp4DemuxerConfiguration;

        public UIElement Create(ComponentConfiguration configuration) => new Mp4DemuxerConfigurationControl() {
            DataContext = configuration,
        };
    }
}
