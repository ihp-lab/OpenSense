#nullable enable

using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using OpenSense.Components.HM;

namespace OpenSense.WPF.Components.HM {
    /// <summary>
    /// Reusable control that visualizes bit depth mapping.
    /// All rows in a single Grid for guaranteed column alignment.
    /// </summary>
    public sealed partial class BitDepthMappingPreview : UserControl {

        private static readonly int[] DefaultSourceBitDepths = { 8, 10, 12, 14, 16 };

        private const double BarHeight = 14;

        #region Dependency Properties

        public static readonly DependencyProperty TargetBitDepthProperty = DependencyProperty.Register(nameof(TargetBitDepth), typeof(int), typeof(BitDepthMappingPreview), new PropertyMetadata(8, OnParameterChanged));
        public static readonly DependencyProperty ScaleShiftProperty = DependencyProperty.Register(nameof(ScaleShift), typeof(int), typeof(BitDepthMappingPreview), new PropertyMetadata(0, OnParameterChanged));
        public static readonly DependencyProperty WindowStartProperty = DependencyProperty.Register(nameof(WindowStart), typeof(int), typeof(BitDepthMappingPreview), new PropertyMetadata(0, OnParameterChanged));
        public static readonly DependencyProperty SourceBitDepthProperty = DependencyProperty.Register(nameof(SourceBitDepth), typeof(int), typeof(BitDepthMappingPreview), new PropertyMetadata(0, OnParameterChanged));

        public int TargetBitDepth {
            get => (int)GetValue(TargetBitDepthProperty);
            set => SetValue(TargetBitDepthProperty, value);
        }

        public int ScaleShift {
            get => (int)GetValue(ScaleShiftProperty);
            set => SetValue(ScaleShiftProperty, value);
        }

        public int WindowStart {
            get => (int)GetValue(WindowStartProperty);
            set => SetValue(WindowStartProperty, value);
        }

        public int SourceBitDepth {
            get => (int)GetValue(SourceBitDepthProperty);
            set => SetValue(SourceBitDepthProperty, value);
        }

        #endregion

        public BitDepthMappingPreview() {
            InitializeComponent();
            Loaded += (_, _) => Refresh();
        }

        #region Rendering

        private static void OnParameterChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
            ((BitDepthMappingPreview)d).Refresh();
        }

        private void Refresh() {
            PreviewPanel.Children.Clear();

            var targetBits = TargetBitDepth;
            var scaleShift = ScaleShift;
            var windowStart = WindowStart;
            var windowSize = BitDepthMappingInfo.GetWindowSize(targetBits, scaleShift);
            var windowEnd = windowStart + windowSize;

            // Build a single Grid for all rows
            var grid = new Grid();
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

            var row = 0;

            // Source rows
            var srcBits = SourceBitDepth > 0 ? new[] { SourceBitDepth } : DefaultSourceBitDepths;
            foreach (var bits in srcBits) {
                grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
                AddSourceRow(grid, row, bits, windowStart, windowEnd, windowSize);
                row++;
            }

            // Arrow row
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            var arrow = new TextBlock {
                Text = "↓",
                FontSize = 11,
                HorizontalAlignment = HorizontalAlignment.Center,
                Margin = new Thickness(0, 2, 0, 2),
            };
            Grid.SetRow(arrow, row);
            Grid.SetColumn(arrow, 2);
            grid.Children.Add(arrow);
            row++;

            // Target row
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            AddTargetRow(grid, row, targetBits, scaleShift, windowSize);

            PreviewPanel.Children.Add(grid);
        }

        private static void AddSourceRow(Grid grid, int row, int srcBits, int windowStart, long windowEnd, int windowSize) {
            var srcMax = 1L << srcBits;

            AddCell(grid, row, 0, CreateLabel($"{srcBits}-bit"));

            if (windowSize <= 0) {
                AddCell(grid, row, 1, CreateRangeLabel("–", Brushes.Red));
                AddBarWithError(grid, row, "invalid window");
            } else if (windowStart < 0 || windowEnd > srcMax) {
                AddCell(grid, row, 1, CreateRangeLabel($"{windowStart}–{windowEnd - 1}", Brushes.Red));
                AddBarWithError(grid, row, "window > source");
            } else {
                AddCell(grid, row, 1, CreateRangeLabel($"{windowStart}–{windowEnd - 1}", Brushes.Gray));
                var winStartFrac = (double)windowStart / srcMax;
                var winEndFrac = (double)windowEnd / srcMax;
                AddBar(grid, row, Brushes.CornflowerBlue, winStartFrac, winEndFrac);
            }
        }

        private static void AddTargetRow(Grid grid, int row, int targetBits, int scaleShift, int windowSize) {
            var targetMax = 1L << targetBits;

            AddCell(grid, row, 0, CreateLabel($"{targetBits}-bit"));

            if (windowSize <= 0) {
                AddCell(grid, row, 1, CreateRangeLabel("–", Brushes.Gray));
                AddBarWithError(grid, row, "");
            } else {
                long maxOutput;
                if (scaleShift >= 0) {
                    maxOutput = Math.Min((windowSize - 1) >> scaleShift, targetMax - 1);
                } else {
                    maxOutput = Math.Min(((long)windowSize - 1) << (-scaleShift), targetMax - 1);
                }
                var outEndFrac = (double)(maxOutput + 1) / targetMax;

                AddCell(grid, row, 1, CreateRangeLabel($"0–{maxOutput}", Brushes.Gray));
                AddBar(grid, row, Brushes.MediumSeaGreen, 0, outEndFrac);
            }
        }

        #endregion

        #region UI Helpers

        private static TextBlock CreateLabel(string text) {
            return new TextBlock {
                Text = text,
                FontSize = 11,
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(0, 0, 4, 0),
            };
        }

        private static TextBlock CreateRangeLabel(string text, Brush foreground) {
            return new TextBlock {
                Text = text,
                Foreground = foreground,
                FontSize = 11,
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(0, 0, 4, 0),
            };
        }

        private static void AddCell(Grid grid, int row, int col, UIElement element) {
            Grid.SetRow(element, row);
            Grid.SetColumn(element, col);
            grid.Children.Add(element);
        }

        private static void AddBar(Grid grid, int row, Brush highlightBrush, double startFrac, double endFrac) {
            var innerGrid = new Grid {
                Height = BarHeight,
                Background = Brushes.LightGray,
            };

            var rect = new Rectangle {
                Fill = highlightBrush,
                HorizontalAlignment = HorizontalAlignment.Left,
            };
            innerGrid.Children.Add(rect);

            innerGrid.SizeChanged += (_, args) => {
                var totalWidth = args.NewSize.Width;
                rect.Width = Math.Max((endFrac - startFrac) * totalWidth, 0);
                rect.Margin = new Thickness(startFrac * totalWidth, 0, 0, 0);
            };

            var bar = new Border {
                BorderBrush = Brushes.DarkGray,
                BorderThickness = new Thickness(1),
                VerticalAlignment = VerticalAlignment.Center,
                Child = innerGrid,
            };

            Grid.SetRow(bar, row);
            Grid.SetColumn(bar, 2);
            grid.Children.Add(bar);
        }

        private static void AddBarWithError(Grid grid, int row, string reason) {
            var innerGrid = new Grid {
                Height = BarHeight,
                Background = Brushes.LightGray,
            };

            if (!string.IsNullOrEmpty(reason)) {
                innerGrid.Children.Add(new TextBlock {
                    Text = reason,
                    Foreground = Brushes.Red,
                    FontSize = 10,
                    VerticalAlignment = VerticalAlignment.Center,
                    HorizontalAlignment = HorizontalAlignment.Center,
                });
            }

            var bar = new Border {
                BorderBrush = Brushes.DarkGray,
                BorderThickness = new Thickness(1),
                VerticalAlignment = VerticalAlignment.Center,
                Child = innerGrid,
            };

            Grid.SetRow(bar, row);
            Grid.SetColumn(bar, 2);
            grid.Children.Add(bar);
        }

        #endregion
    }
}
