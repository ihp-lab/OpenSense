using System.Windows;

namespace OpenSense.Wpf.Widget.Contract {
    public interface IWidgetMetadata {

        string Name { get; }

        string Description { get; }

        Window Create();
    }
}
