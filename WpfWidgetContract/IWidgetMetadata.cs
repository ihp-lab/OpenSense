using System.Windows;

namespace OpenSense.WPF.Widgets {
    public interface IWidgetMetadata {

        string Name { get; }

        string Description { get; }

        Window Create();
    }
}
