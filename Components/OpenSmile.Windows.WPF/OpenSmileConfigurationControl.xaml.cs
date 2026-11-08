using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using OpenSense.Components.OpenSmile;
using OpenSmileInterop;

namespace OpenSense.WPF.Components.OpenSmile;

public sealed partial class OpenSmileConfigurationControl : UserControl {

    private OpenSmileConfiguration? Configuration => DataContext as OpenSmileConfiguration;

    public OpenSmileConfigurationControl() {
        InitializeComponent();
    }

    #region Control Event Handlers
    private void ButtonOpenFile_Click(object sender, RoutedEventArgs e) {
        if (Configuration is not { } config) {
            return;
        }
        var openFileDialog = new Microsoft.Win32.OpenFileDialog {
            CheckFileExists = true,
            AddExtension = true,
            DefaultExt = "conf",
            Filter = "openSMILE Configuration (*.conf)|*.conf|All Files (*.*)|*.*",
        };
        if (openFileDialog.ShowDialog() == true) {
            config.ConfigurationFilename = openFileDialog.FileName;
        }
    }

    private async void ButtonInspect_Click(object sender, RoutedEventArgs e) {
        if (Configuration is not { } config) {
            return;
        }
        var filename = config.ConfigurationFilename;
        var options = config.Options.Select(o => new KeyValuePair<string, string>(o.Name, o.Value)).ToArray();
        var logLevel = config.OpenSmileLogLevel;

        ButtonInspect.IsEnabled = false;
        PanelInspection.Children.Clear();
        PanelInspection.Children.Add(new TextBlock { Text = "Loading...", Foreground = Brushes.Gray });
        try {
            // Port names and feature names are only known after openSMILE has built the whole processing chain.
            ShowInspection(await Task.Run(() => Inspect(filename, options, logLevel)));
        } finally {
            ButtonInspect.IsEnabled = true;
        }
    }
    #endregion

    #region Inspection
    private sealed record class PortDescription(string Name, string Summary, string? Details);

    private sealed record class Inspection(IReadOnlyList<PortDescription> Inputs, IReadOnlyList<PortDescription> Outputs, string? Error);

    private static Inspection Inspect(string filename, IReadOnlyList<KeyValuePair<string, string>> options, int logLevel) {
        try {
            // Errors are reported by the exception, so log messages are not needed here.
            using var extractor = new Extractor(filename, options, logLevel, null);
            var inputs = extractor.Sources
                .Select(s => new PortDescription(s.Name, $"{s.SampleRate} Hz, {Count(s.Channels, "channel")}, {s.BitsPerSample}-bit {(s.IsFloat ? "float" : "PCM")}", null))
                .ToArray();
            var outputs = extractor.Sinks
                .Select(s => new PortDescription(s.Name, Count(s.ElementNames.Count, "feature"), string.Join(", ", s.ElementNames)))
                .ToArray();
            return new Inspection(inputs, outputs, null);
        } catch (Exception ex) {
            return new Inspection(Array.Empty<PortDescription>(), Array.Empty<PortDescription>(), ex.Message);
        }
    }

    private static string Count(int count, string noun) => count == 1 ? $"1 {noun}" : $"{count} {noun}s";

    private void ShowInspection(Inspection inspection) {
        PanelInspection.Children.Clear();
        if (inspection.Error is { } error) {
            PanelInspection.Children.Add(new TextBlock { Text = error, TextWrapping = TextWrapping.Wrap, Foreground = Brushes.DarkRed });
            return;
        }
        AddPortSection("Input Ports", inspection.Inputs);
        AddPortSection("Output Ports", inspection.Outputs);
    }

    private void AddPortSection(string title, IReadOnlyList<PortDescription> ports) {
        var isFirst = PanelInspection.Children.Count == 0;
        PanelInspection.Children.Add(new TextBlock {
            Text = title,
            Foreground = Brushes.Gray,
            Margin = new Thickness(0, isFirst ? 0 : 6, 0, 2),
        });
        if (ports.Count == 0) {
            PanelInspection.Children.Add(new TextBlock { Text = "None", Margin = new Thickness(8, 0, 0, 0) });
            return;
        }

        // A two-column grid keeps wrapped descriptions aligned in their own column instead of flowing under the port name.
        var grid = new Grid { Margin = new Thickness(8, 0, 0, 0) };
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
        grid.ColumnDefinitions.Add(new ColumnDefinition());
        foreach (var port in ports) {
            var row = grid.RowDefinitions.Count;
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

            var name = new TextBlock { Text = port.Name, FontWeight = FontWeights.SemiBold, Margin = new Thickness(0, 0, 12, 2) };
            Grid.SetRow(name, row);
            grid.Children.Add(name);

            var description = new TextBlock { TextWrapping = TextWrapping.Wrap, Margin = new Thickness(0, 0, 0, 2) };
            description.Inlines.Add(new Run(port.Summary));
            if (!string.IsNullOrEmpty(port.Details)) {
                description.Inlines.Add(new LineBreak());
                description.Inlines.Add(new Run(port.Details) { Foreground = Brushes.Gray });
            }
            Grid.SetRow(description, row);
            Grid.SetColumn(description, 1);
            grid.Children.Add(description);
        }
        PanelInspection.Children.Add(grid);
    }
    #endregion
}
