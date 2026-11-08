using System.Linq;
using System.Windows.Controls;

namespace OpenSense.WPF.Components.OpenSmile.Visualizer;

public sealed partial class OpenSmileVisualizerInstanceControl : UserControl {

    public OpenSmileVisualizerInstanceControl() {
        InitializeComponent();
    }

    #region Control Event Handlers
    // ListBox.SelectedItems cannot be bound, so the selection is passed to the plot here.
    private void ListBoxFeatures_SelectionChanged(object sender, SelectionChangedEventArgs e) {
        Plot.SelectedIndices = ListBoxFeatures.SelectedItems
            .Cast<object>()
            .Select(item => ListBoxFeatures.Items.IndexOf(item))
            .OrderBy(index => index)
            .ToArray();
    }
    #endregion
}
