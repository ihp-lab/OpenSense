using System;
using System.ComponentModel;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;
using OpenSense.Components.LibreFace.Visualizer;

namespace OpenSense.WPF.Components.LibreFace {
    public partial class ActionUnitIntensityVisualizerInstanceControl : UserControl {

        private static readonly TimeSpan TimeOut = TimeSpan.FromMilliseconds(100);

        private ActionUnitIntensityVisualizer Instance => DataContext as ActionUnitIntensityVisualizer;

        private int rangeTo = 1;

        public ActionUnitIntensityVisualizerInstanceControl() {
            InitializeComponent();
        }

        private void UserControl_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e) {
            if (e.OldValue is ActionUnitIntensityVisualizer old) {
                old.PropertyChanged -= OnPropertyChanged;
            }
            if (e.NewValue is ActionUnitIntensityVisualizer @new) {
                @new.PropertyChanged += OnPropertyChanged;
            }
        }

        private void OnPropertyChanged(object sender, PropertyChangedEventArgs args) {
            if (args.PropertyName != nameof(ActionUnitIntensityVisualizer.Last)) {
                return;
            }
            var dict = ((ActionUnitIntensityVisualizer)sender).Last;
            /* Timeout is required, otherwise when disposing pipeline, it will stuck. */
            try {
                Dispatcher.Invoke(() => {
                    var numCols = GridMain.ColumnDefinitions.Count;
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
                            GridMain.Children.RemoveRange(GridMain.Children.Count - numCols, numCols);
                        }
                    }
                    var idx = 0;
                    foreach (var (id, val) in dict) {
                        ((TextBlock)GridMain.Children[idx * numCols]).Text = id.ToString();
                        var bar = (ProgressBar)GridMain.Children[idx * numCols + 1];
                        bar.Value = rangeTo - val;
                        ((TextBlock)GridMain.Children[idx * numCols + 2]).Text = val.ToString("F2");
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
            rangeTo = int.Parse(tag);
            foreach (var child in GridMain.Children) {
                if (child is ProgressBar bar) {
                    bar.Maximum = rangeTo;
                }
            }
        }
    }
}
