using System.Windows;

namespace OpenSense.WPF.Widget.Contract {
    public interface IWidgetMetadata {

        string Name { get; }

        string Description { get; }

        Window Create();
    }
}
