#nullable enable

using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace OpenSense.WPF.Components.Kvazaar {
    /// <summary>
    /// Reusable control that visualizes bit depth mapping.
    /// Shows paired input/output bars for each possible source bit depth.
    /// </summary>
    public sealed partial class BitDepthMappingPreview : UserControl {

        private static readonly int[] DefaultSourceBitDepths = { 8, 16 };

        private const double BarHeight = 18;

        #region Dependency Properties

        public static readonly DependencyProperty TargetBitDepthProperty = DependencyProperty.Register(nameof(TargetBitDepth), typeof(int), typeof(BitDepthMappingPreview), new PropertyMetadata(8, OnParameterChanged));
        public static readonly DependencyProperty ScaleShiftProperty = DependencyProperty.Register(nameof(ScaleShift), typeof(int), typeof(BitDepthMappingPreview), new PropertyMetadata(0, OnParameterChanged));
        public static readonly DependencyProperty InputStartProperty = DependencyProperty.Register(nameof(InputStart), typeof(int), typeof(BitDepthMappingPreview), new PropertyMetadata(0, OnParameterChanged));
        public static readonly DependencyProperty OutputStartProperty = DependencyProperty.Register(nameof(OutputStart), typeof(int), typeof(BitDepthMappingPreview), new PropertyMetadata(0, OnParameterChanged));
        public static readonly DependencyProperty SourceBitDepthProperty = DependencyProperty.Register(nameof(SourceBitDepth), typeof(int), typeof(BitDepthMappingPreview), new PropertyMetadata(0, OnParameterChanged));
        public static readonly DependencyProperty EffectiveSourceBitDepthProperty = DependencyProperty.Register(nameof(EffectiveSourceBitDepth), typeof(int), typeof(BitDepthMappingPreview), new PropertyMetadata(16, OnParameterChanged));

        public int TargetBitDepth {
            get => (int)GetValue(TargetBitDepthProperty);
            set => SetValue(TargetBitDepthProperty, value);
        }

        public int ScaleShift {
            get => (int)GetValue(ScaleShiftProperty);
            set => SetValue(ScaleShiftProperty, value);
        }

        public int InputStart {
            get => (int)GetValue(InputStartProperty);
            set => SetValue(InputStartProperty, value);
        }

        public int OutputStart {
            get => (int)GetValue(OutputStartProperty);
            set => SetValue(OutputStartProperty, value);
        }

        public int SourceBitDepth {
            get => (int)GetValue(SourceBitDepthProperty);
            set => SetValue(SourceBitDepthProperty, value);
        }

        public int EffectiveSourceBitDepth {
            get => (int)GetValue(EffectiveSourceBitDepthProperty);
            set => SetValue(EffectiveSourceBitDepthProperty, value);
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
            var inputStart = InputStart;
            var outputStart = OutputStart;

            var effectiveBits = EffectiveSourceBitDepth;
            var srcBits = SourceBitDepth > 0 ? new[] { SourceBitDepth } : DefaultSourceBitDepths;

            for (var i = 0; i < srcBits.Length; i++) {
                if (i > 0) {
                    PreviewPanel.Children.Add(new Separator { Margin = new Thickness(0, 4, 0, 4) });
                }
                AddPair(srcBits[i], targetBits, scaleShift, inputStart, outputStart, effectiveBits);
            }
        }

        private void AddPair(int srcBits, int targetBits, int scaleShift, int inputStart, int outputStart, int effectiveBits) {
            var effectiveSrcBits = Math.Min(srcBits, effectiveBits);
            var fullSrcMax = 1L << srcBits;
            var effectiveSrcMax = 1L << effectiveSrcBits;
            var targetMax = 1L << targetBits;

            // Input bar
            var inputGrid = new Grid();
            inputGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(55) });
            inputGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

            var inputLabel = new TextBlock {
                Text = effectiveSrcBits < srcBits ? $"{srcBits}-bit in (eff. {effectiveSrcBits})" : $"{srcBits}-bit in",
                FontSize = 11,
                VerticalAlignment = VerticalAlignment.Center,
                FontFamily = new FontFamily("Consolas"),
            };
            Grid.SetColumn(inputLabel, 0);
            inputGrid.Children.Add(inputLabel);

            var inputValid = inputStart >= 0 && inputStart < fullSrcMax;

            if (!inputValid) {
                var errorBar = CreateErrorBar("out of range");
                Grid.SetColumn(errorBar, 1);
                inputGrid.Children.Add(errorBar);
            } else {
                var windowEnd = Math.Min(inputStart + effectiveSrcMax - 1, fullSrcMax - 1);
                var startFrac = (double)inputStart / fullSrcMax;
                var endFrac = (double)(windowEnd + 1) / fullSrcMax;
                var bar = CreateBar(Brushes.CornflowerBlue, startFrac, endFrac, $"{inputStart}", $"{windowEnd}");
                Grid.SetColumn(bar, 1);
                inputGrid.Children.Add(bar);
            }

            PreviewPanel.Children.Add(inputGrid);

            // Output bar
            var outputGrid = new Grid();
            outputGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(55) });
            outputGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

            var outputLabel = new TextBlock {
                Text = $"{targetBits}-bit out",
                FontSize = 11,
                VerticalAlignment = VerticalAlignment.Center,
                FontFamily = new FontFamily("Consolas"),
            };
            Grid.SetColumn(outputLabel, 0);
            outputGrid.Children.Add(outputLabel);

            if (!inputValid) {
                var errorBar = CreateErrorBar("");
                Grid.SetColumn(errorBar, 1);
                outputGrid.Children.Add(errorBar);
            } else {
                var sourceSpan = Math.Min(effectiveSrcMax - 1, fullSrcMax - 1 - inputStart);
                long rawMax;
                if (scaleShift >= 0) {
                    rawMax = (sourceSpan >> scaleShift) + outputStart;
                } else {
                    rawMax = (sourceSpan << (-scaleShift)) + outputStart;
                }
                var outStart = (long)outputStart;
                var clamped = rawMax > targetMax - 1;
                var outEnd = Math.Min(rawMax, targetMax - 1);
                var outStartFrac = (double)outStart / targetMax;
                var outEndFrac = (double)(outEnd + 1) / targetMax;

                var endText = clamped ? $"{outEnd}!" : $"{outEnd}";
                var endBrush = clamped ? Brushes.Red : Brushes.White;
                var bar = CreateBar(Brushes.MediumSeaGreen, outStartFrac, outEndFrac, $"{outStart}", endText, endBrush);
                Grid.SetColumn(bar, 1);
                outputGrid.Children.Add(bar);
            }

            PreviewPanel.Children.Add(outputGrid);
        }

        #endregion

        #region UI Helpers

        private static Border CreateBar(Brush highlightBrush, double startFrac, double endFrac, string startText, string endText, Brush? endTextBrush = null) {
            var innerGrid = new Grid {
                Height = BarHeight,
                Background = Brushes.LightGray,
            };

            var rect = new Rectangle {
                Fill = highlightBrush,
                HorizontalAlignment = HorizontalAlignment.Left,
            };
            innerGrid.Children.Add(rect);

            var textPanel = new DockPanel {
                HorizontalAlignment = HorizontalAlignment.Left,
            };
            var leftText = new TextBlock {
                Text = startText,
                FontSize = 10,
                FontFamily = new FontFamily("Consolas"),
                Foreground = Brushes.White,
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(2, 0, 4, 0),
            };
            var rightText = new TextBlock {
                Text = endText,
                FontSize = 10,
                FontFamily = new FontFamily("Consolas"),
                Foreground = endTextBrush ?? Brushes.White,
                VerticalAlignment = VerticalAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Right,
                Margin = new Thickness(4, 0, 2, 0),
            };
            DockPanel.SetDock(leftText, Dock.Left);
            DockPanel.SetDock(rightText, Dock.Right);
            textPanel.Children.Add(leftText);
            textPanel.Children.Add(rightText);
            innerGrid.Children.Add(textPanel);

            innerGrid.SizeChanged += (_, args) => {
                var totalWidth = args.NewSize.Width;
                var highlightWidth = Math.Max((endFrac - startFrac) * totalWidth, 0);
                rect.Width = highlightWidth;
                rect.Margin = new Thickness(startFrac * totalWidth, 0, 0, 0);
                textPanel.Width = highlightWidth;
                textPanel.Margin = new Thickness(startFrac * totalWidth, 0, 0, 0);
            };

            return new Border {
                BorderBrush = Brushes.DarkGray,
                BorderThickness = new Thickness(1),
                VerticalAlignment = VerticalAlignment.Center,
                Child = innerGrid,
            };
        }

        private static Border CreateErrorBar(string reason) {
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

            return new Border {
                BorderBrush = Brushes.DarkGray,
                BorderThickness = new Thickness(1),
                VerticalAlignment = VerticalAlignment.Center,
                Child = innerGrid,
            };
        }

        #endregion
    }
}
