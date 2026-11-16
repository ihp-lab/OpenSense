#nullable enable

using System.Composition;
using System.Windows;
using OpenSense.Components;
using OpenSense.Components.Psi.Imaging;

namespace OpenSense.WPF.Components.Psi.Imaging;

[Export(typeof(IConfigurationControlCreator))]
public sealed class CropImageOperatorConfigurationControlCreator : IConfigurationControlCreator {

    public bool CanCreate(ComponentConfiguration configuration) => configuration is CropImageOperatorConfiguration;

    public UIElement Create(ComponentConfiguration configuration) => new CropImageOperatorConfigurationControl() {
        DataContext = configuration,
    };
}
