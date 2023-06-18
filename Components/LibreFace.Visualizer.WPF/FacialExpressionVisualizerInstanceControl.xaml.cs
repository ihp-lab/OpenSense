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

        private const int RangeTo = 1;

        private static readonly TimeSpan TimeOut = TimeSpan.FromMilliseconds(100);

        private FacialExpressionVisualizer Instance => DataContext as FacialExpressionVisualizer;

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
                            pb.Maximum = RangeTo;

                            // see https://stackoverflow.com/questions/24288870/c-sharp-xaml-progressbar-set-gradient-filling-properly
                            var brush = new LinearGradientBrush();
                            brush.GradientStops.Add(new GradientStop { Color = Colors.Red, Offset = 0, });
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
                    var idx = 0;
                    foreach (var (id, val) in dict.OrderBy(kv => kv.Key)) {//Order by keys
                        ((TextBlock)GridMain.Children[idx * 3]).Text = id.ToString();
                        var bar = (ProgressBar)GridMain.Children[idx * 3 + 1];
                        bar.Value = RangeTo - val;
                        ((TextBlock)GridMain.Children[idx * 3 + 2]).Text = val.ToString("F2");
                        idx++;
                    }
                }, DispatcherPriority.DataBind, CancellationToken.None, TimeOut);
            } catch (TimeoutException) {
                ;//Nothing
            }
        }
    }
}
