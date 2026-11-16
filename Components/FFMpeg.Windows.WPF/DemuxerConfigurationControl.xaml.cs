#nullable enable

using System.Windows;
using System.Windows.Controls;
using OpenSense.Components.FFMpeg;

namespace OpenSense.WPF.Components.FFMpeg;

public sealed partial class DemuxerConfigurationControl : UserControl {

    private DemuxerConfiguration? Configuration => DataContext as DemuxerConfiguration;

    public DemuxerConfigurationControl() {
        InitializeComponent();
    }

    #region Control Event Handlers
    private void ButtonOpenFile_Click(object sender, RoutedEventArgs e) {
        if (Configuration is not { } config) {
            return;
        }
        var openFileDialog = new Microsoft.Win32.OpenFileDialog {
            CheckFileExists = true,
            Filter = "All Files (*.*)|*.*",
        };
        if (openFileDialog.ShowDialog() == true) {
            config.Filename = openFileDialog.FileName;
        }
    }
    #endregion
}
