using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using System.Windows.Threading;
using OpenSense.Components.OpenSmile.Visualizer;

namespace OpenSense.WPF.Components.OpenSmile.Visualizer;

/// <summary>
/// Minimal line plot of selected features over the visualizer's time window.
/// It polls the visualizer on a timer, so the pipeline never waits for the UI.
/// </summary>
public sealed class FeaturePlot : FrameworkElement {

    #region Styles
    private static readonly Brush[] SeriesBrushes = {
        Brushes.RoyalBlue,
        Brushes.OrangeRed,
        Brushes.ForestGreen,
        Brushes.DarkOrchid,
        Brushes.Goldenrod,
        Brushes.Teal,
        Brushes.Crimson,
        Brushes.SaddleBrown,
    };

    private const double AxisMargin = 48;
    private const double LabelSize = 11;

    private static readonly Pen FramePen = CreateFramePen();
    private static readonly Brush LegendBackground = CreateLegendBackground();

    private static Pen CreateFramePen() {
        var pen = new Pen(new SolidColorBrush(Color.FromRgb(0xD0, 0xD0, 0xD0)), 1);
        pen.Freeze();
        return pen;
    }

    private static Brush CreateLegendBackground() {
        var brush = new SolidColorBrush(Color.FromArgb(0xC0, 0xFF, 0xFF, 0xFF));
        brush.Freeze();
        return brush;
    }
    #endregion

    private readonly DispatcherTimer _timer;
    private int drawnVersion = -1;

    #region Dependency Properties
    public static readonly DependencyProperty SourceProperty = DependencyProperty.Register(nameof(Source), typeof(OpenSmileVisualizer), typeof(FeaturePlot), new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsRender));
    public static readonly DependencyProperty SelectedIndicesProperty = DependencyProperty.Register(nameof(SelectedIndices), typeof(IReadOnlyList<int>), typeof(FeaturePlot), new FrameworkPropertyMetadata(Array.Empty<int>(), FrameworkPropertyMetadataOptions.AffectsRender));

    public OpenSmileVisualizer? Source {
        get => (OpenSmileVisualizer?)GetValue(SourceProperty);
        set => SetValue(SourceProperty, value);
    }

    public IReadOnlyList<int> SelectedIndices {
        get => (IReadOnlyList<int>)GetValue(SelectedIndicesProperty);
        set => SetValue(SelectedIndicesProperty, value);
    }
    #endregion

    public FeaturePlot() {
        _timer = new DispatcherTimer(TimeSpan.FromMilliseconds(50), DispatcherPriority.Render, OnTick, Dispatcher);
        _timer.Stop();
        Loaded += (_, _) => _timer.Start();
        // Stopping on unload releases the timer's reference to this element.
        Unloaded += (_, _) => _timer.Stop();
    }

    #region Refresh
    private void OnTick(object? sender, EventArgs e) {
        // Property changes redraw by themselves; only new data from the pipeline has to be polled.
        if (Source is { } source && source.Version != drawnVersion) {
            InvalidateVisual();
        }
    }
    #endregion

    #region Rendering
    protected override void OnRender(DrawingContext context) {
        var source = Source;
        var selectedIndices = SelectedIndices;
        var area = new Rect(AxisMargin, LabelSize, Math.Max(0, ActualWidth - AxisMargin - 4), Math.Max(0, ActualHeight - 2 * LabelSize - 6));
        context.DrawRectangle(Brushes.White, FramePen, area);
        if (source is null || area.Width <= 0 || area.Height <= 0) {
            return;
        }

        // Read before the snapshot, so that data arriving in between triggers another redraw.
        drawnVersion = source.Version;
        var samples = source.GetSnapshot();
        var indices = selectedIndices.Where(i => samples.Length == 0 || i < samples[0].Values.Length).ToArray();
        if (indices.Length == 0 || samples.Length == 0) {
            DrawText(context, indices.Length == 0 ? "Select features on the left." : "Waiting for data...", new Point(area.Left + 6, area.Top + 6), Brushes.Gray);
            return;
        }

        var end = samples[^1].Time;
        var window = source.Window;
        var start = end - window;
        var values = samples.SelectMany(s => indices.Select(i => (double)s.Values[i])).Where(v => !double.IsNaN(v) && !double.IsInfinity(v)).ToArray();
        var min = values.Length > 0 ? values.Min() : 0;
        var max = values.Length > 0 ? values.Max() : 0;
        if (max - min < 1e-12) {
            // A constant feature still needs a non-empty range to be drawn.
            min -= 1;
            max += 1;
        }

        double X(DateTime time) => area.Left + (time - start).TotalSeconds / window.TotalSeconds * area.Width;
        double Y(double value) => area.Bottom - (value - min) / (max - min) * area.Height;

        // Labels lie outside the plot area, so they are drawn before clipping to it.
        DrawText(context, FormatValue(max), new Point(2, area.Top - LabelSize / 2), Brushes.Black);
        DrawText(context, FormatValue(min), new Point(2, area.Bottom - LabelSize / 2), Brushes.Black);
        DrawText(context, $"-{window.TotalSeconds:0.#} s", new Point(area.Left, area.Bottom + 2), Brushes.Black);
        DrawText(context, "0 s", new Point(area.Right - 16, area.Bottom + 2), Brushes.Black);

        context.PushClip(new RectangleGeometry(area));
        for (var s = 0; s < indices.Length; s++) {
            var brush = SeriesBrushes[s % SeriesBrushes.Length];
            var pen = new Pen(brush, 1.5);
            var points = samples.Where(sample => float.IsFinite(sample.Values[indices[s]])).Select(sample => new Point(X(sample.Time), Y(sample.Values[indices[s]]))).ToArray();
            if (points.Length == 1) {
                // A single frame, e.g. functionals over the whole input, is shown as a dot.
                context.DrawEllipse(brush, null, points[0], 3, 3);
                continue;
            }
            var geometry = new StreamGeometry();
            using (var geometryContext = geometry.Open()) {
                geometryContext.BeginFigure(points[0], isFilled: false, isClosed: false);
                geometryContext.PolyLineTo(points.Skip(1).ToArray(), isStroked: true, isSmoothJoin: false);
            }
            geometry.Freeze();
            context.DrawGeometry(null, pen, geometry);
        }
        context.Pop();

        // Legend, on a translucent background so that it stays readable over the lines
        var names = source.FeatureNames;
        var legend = new FormattedText[indices.Length];
        for (var s = 0; s < indices.Length; s++) {
            var index = indices[s];
            var name = index < names.Count ? names[index] : $"#{index}";
            var latest = samples[^1].Values[index];
            legend[s] = CreateText($"{name}: {FormatValue(latest)}", SeriesBrushes[s % SeriesBrushes.Length]);
        }
        var lineHeight = LabelSize + 3;
        var legendWidth = legend.Max(t => t.WidthIncludingTrailingWhitespace);
        context.DrawRectangle(LegendBackground, null, new Rect(area.Left + 1, area.Top + 1, legendWidth + 10, legend.Length * lineHeight + 6));
        for (var s = 0; s < legend.Length; s++) {
            context.DrawText(legend[s], new Point(area.Left + 6, area.Top + 4 + s * lineHeight));
        }
    }

    private FormattedText CreateText(string text, Brush brush) {
        return new FormattedText(text, CultureInfo.CurrentUICulture, FlowDirection.LeftToRight, new Typeface("Segoe UI"), LabelSize, brush, VisualTreeHelper.GetDpi(this).PixelsPerDip);
    }

    private void DrawText(DrawingContext context, string text, Point origin, Brush brush) {
        context.DrawText(CreateText(text, brush), origin);
    }

    private static string FormatValue(double value) => value.ToString("G4", CultureInfo.InvariantCulture);
    #endregion
}
