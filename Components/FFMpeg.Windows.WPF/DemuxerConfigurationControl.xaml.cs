#nullable enable

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
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

    private async void ButtonInspect_Click(object sender, RoutedEventArgs e) {
        if (Configuration is not { } config) {
            return;
        }
        var filename = config.Filename;

        ButtonInspect.IsEnabled = false;
        PanelInspection.Children.Clear();
        PanelInspection.Children.Add(new TextBlock { Text = "Loading...", Foreground = Brushes.Gray });
        try {
            // Probing the streams reads from the file, which can be slow on network or removable drives.
            ShowInspection(await Task.Run(() => Inspect(filename)));
        } finally {
            ButtonInspect.IsEnabled = true;
        }
    }
    #endregion

    #region Inspection
    private sealed record class StreamDescription(int Index, string Summary);

    private sealed record class Inspection(IReadOnlyList<StreamDescription> Video, IReadOnlyList<StreamDescription> Audio, string? Error);

    private static Inspection Inspect(string filename) {
        try {
            using var demuxer = new FFMpegInterop.Demuxer(filename);
            var video = demuxer.VideoStreams
                .Select((s, i) => new StreamDescription(i, $"{s.CodecName}, {s.Width}x{s.Height}"))
                .ToArray();
            var audio = demuxer.AudioStreams
                .Select((s, i) => new StreamDescription(i, $"{s.CodecName}, {s.SampleRate} Hz, {Count(s.ChannelCount, "channel")}"))
                .ToArray();
            return new Inspection(video, audio, null);
        } catch (Exception ex) {
            return new Inspection(Array.Empty<StreamDescription>(), Array.Empty<StreamDescription>(), ex.Message);
        }
    }

    private static string Count(int count, string noun) => count == 1 ? $"1 {noun}" : $"{count} {noun}s";

    private void ShowInspection(Inspection inspection) {
        PanelInspection.Children.Clear();
        if (inspection.Error is { } error) {
            PanelInspection.Children.Add(new TextBlock { Text = error, TextWrapping = TextWrapping.Wrap, Foreground = Brushes.DarkRed });
            return;
        }
        AddStreamSection(nameof(Demuxer.VideoOut), inspection.Video);
        AddStreamSection(nameof(Demuxer.AudioOut), inspection.Audio);
    }

    private void AddStreamSection(string title, IReadOnlyList<StreamDescription> streams) {
        var isFirst = PanelInspection.Children.Count == 0;
        PanelInspection.Children.Add(new TextBlock {
            Text = title,
            Foreground = Brushes.Gray,
            Margin = new Thickness(0, isFirst ? 0 : 6, 0, 2),
        });
        if (streams.Count == 0) {
            PanelInspection.Children.Add(new TextBlock { Text = "None", Margin = new Thickness(8, 0, 0, 0) });
            return;
        }

        // A two-column grid keeps the descriptions aligned in their own column.
        var grid = new Grid { Margin = new Thickness(8, 0, 0, 0) };
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
        grid.ColumnDefinitions.Add(new ColumnDefinition());
        foreach (var stream in streams) {
            var row = grid.RowDefinitions.Count;
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

            var index = new TextBlock { Text = stream.Index.ToString(), FontWeight = FontWeights.SemiBold, Margin = new Thickness(0, 0, 12, 2) };
            Grid.SetRow(index, row);
            grid.Children.Add(index);

            var description = new TextBlock { Text = stream.Summary, TextWrapping = TextWrapping.Wrap, Margin = new Thickness(0, 0, 0, 2) };
            Grid.SetRow(description, row);
            Grid.SetColumn(description, 1);
            grid.Children.Add(description);
        }
        PanelInspection.Children.Add(grid);
    }
    #endregion
}
