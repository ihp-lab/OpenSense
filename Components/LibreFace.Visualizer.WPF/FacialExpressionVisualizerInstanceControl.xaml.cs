using System;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;
using OpenSense.Components.LibreFace.Visualizer;

namespace OpenSense.WPF.Components.LibreFace {
    public partial class FacialExpressionVisualizerInstanceControl : UserControl {

        private static readonly TimeSpan TimeOut = TimeSpan.FromMilliseconds(100);

        private FacialExpressionVisualizer Instance => DataContext as FacialExpressionVisualizer;

        private string rangeMode = "1";

        private float rangeTo = 1;

        public FacialExpressionVisualizerInstanceControl() {
            InitializeComponent();
        }

        private void UserControl_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e) {
            if (e.OldValue is FacialExpressionVisualizer old) {
                old.PropertyChanged -= OnPropertyChanged;
            }
            if (e.NewValue is FacialExpressionVisualizer @new) {
                @new.PropertyChanged += OnPropertyChanged;
            }
        }

        private void OnPropertyChanged(object sender, PropertyChangedEventArgs args) {
            if (args.PropertyName != nameof(FacialExpressionVisualizer.Last)) {
                return;
            }
            var dict = ((FacialExpressionVisualizer)sender).Last;
            /* Timeout is required, otherwise when disposing pipeline, it will stuck. */
            try {
                Dispatcher.Invoke(() => {
                    while (dict.Count != GridMain.RowDefinitions.Count) {
                        if (dict.Count > GridMain.RowDefinitions.Count) {
                            var row = new RowDefinition() {
                                Height = GridLength.Auto,
                            };
                            GridMain.RowDefinitions.Add(row);
                            var rowIdx = GridMain.RowDefinitions.Count - 1;

                            var lb = new TextBlock();
                            GridMain.Children.Add(lb);
                            lb.SetValue(Grid.ColumnProperty, 0);
                            lb.SetValue(Grid.RowProperty, rowIdx);

                            var pb = new ProgressBar();
                            GridMain.Children.Add(pb);
                            pb.SetValue(Grid.ColumnProperty, 1);
                            pb.SetValue(Grid.RowProperty, rowIdx);
                            pb.Minimum = 0;
                            pb.Maximum = rangeTo;

                            // see https://stackoverflow.com/questions/24288870/c-sharp-xaml-progressbar-set-gradient-filling-properly
                            var brush = new LinearGradientBrush();
                            brush.GradientStops.Add(new GradientStop { Color = Colors.Red, Offset = 0, });
                            brush.GradientStops.Add(new GradientStop { Color = Colors.Orange, Offset = 0.5, });
                            brush.GradientStops.Add(new GradientStop { Color = Colors.LightGreen, Offset = 1, });
                            pb.Foreground = pb.Background;
                            pb.Background = brush;
                            pb.FlowDirection = FlowDirection.RightToLeft;

                            var tb = new TextBlock();
                            GridMain.Children.Add(tb);
                            tb.SetValue(Grid.ColumnProperty, 2);
                            tb.SetValue(Grid.RowProperty, rowIdx);
                        }
                        if (dict.Count < GridMain.RowDefinitions.Count) {
                            GridMain.RowDefinitions.RemoveAt(GridMain.RowDefinitions.Count - 1);
                            GridMain.Children.RemoveRange(GridMain.Children.Count - 3, 3);
                        }
                    }
                    rangeTo = rangeMode switch {
                        "1" => 1,
                        "max" => dict.Values.Append(0).Max(),
                        _ => throw new InvalidOperationException(),
                    };
                    var emotionId = dict.MaxBy(kv => kv.Value).Key;
                    var idx = 0;
                    foreach (var (id, val) in dict) {
                        var label = (TextBlock)GridMain.Children[idx * 3];
                        label.FontWeight = id == emotionId ? FontWeights.Bold : FontWeights.Normal;
                        label.Text = id.ToString();

                        var bar = (ProgressBar)GridMain.Children[idx * 3 + 1];
                        bar.Value = rangeTo - val;

                        var valText = (TextBlock)GridMain.Children[idx * 3 + 2];
                        valText.FontWeight = id == emotionId ? FontWeights.Bold : FontWeights.Normal;
                        valText.Text = val.ToString("F2");

                        idx++;
                    }
                }, DispatcherPriority.DataBind, CancellationToken.None, TimeOut);
            } catch (TimeoutException) {
                ;//Nothing
            }
        }

        private void RadioButton_Checked(object sender, RoutedEventArgs e) {
            var tag = (string)((RadioButton)sender).Tag;
            if (tag is null) {
                return;
            }
            rangeMode = tag;
            switch (tag) {
                case "1":
                    rangeTo = 1;
                    break;
                case "max":
                    rangeTo = GridMain
                        .Children
                        .OfType<ProgressBar>()
                        .Select(b => (float)b.Value)
                        .Append(0)
                        .Max()
                        ;
                    break;
                default:
                    throw new InvalidOperationException();
            }
            foreach (var child in GridMain.Children) {
                if (child is ProgressBar bar) {
                    bar.Maximum = rangeTo;
                }
            }
        }

        private void UpdateUpperRange() {

        }
    }
}
