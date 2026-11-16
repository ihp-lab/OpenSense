#nullable enable

using System.Composition;
using System.Windows;
using OpenSense.Components;
using OpenSense.Components.FFMpeg;

namespace OpenSense.WPF.Components.FFMpeg;

[Export(typeof(IConfigurationControlCreator))]
public sealed class AudioDecoderConfigurationControlCreator : IConfigurationControlCreator {

    public bool CanCreate(ComponentConfiguration configuration) => configuration is AudioDecoderConfiguration;

    public UIElement Create(ComponentConfiguration configuration) => new AudioDecoderConfigurationControl() {
        DataContext = configuration,
    };
}
