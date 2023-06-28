using System;
using System.ComponentModel;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using OpenSense.Components.LibreFace.Visualizer;

namespace OpenSense.WPF.Components.LibreFace {
    public partial class ActionUnitPresenceVisualizerInstanceControl : UserControl {

        private static readonly TimeSpan TimeOut = TimeSpan.FromMilliseconds(100);

        private ActionUnitPresenceVisualizer Instance => DataContext as ActionUnitPresenceVisualizer;

        public ActionUnitPresenceVisualizerInstanceControl() {
            InitializeComponent();
        }

        private void UserControl_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e) {
            if (e.OldValue is ActionUnitPresenceVisualizer old) {
                old.PropertyChanged -= OnPropertyChanged;
            }
            if (e.NewValue is ActionUnitPresenceVisualizer @new) {
                @new.PropertyChanged += OnPropertyChanged;
            }
        }

        private void OnPropertyChanged(object sender, PropertyChangedEventArgs args) {
            if (args.PropertyName != nameof(ActionUnitIntensityVisualizer.Last)) {
                return;
            }
            var dict = ((ActionUnitPresenceVisualizer)sender).Last;
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

                            var tb = new TextBlock();
                            GridMain.Children.Add(tb);
                            tb.SetValue(Grid.ColumnProperty, 1);
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
                        ((TextBlock)GridMain.Children[idx * numCols + 1]).Text = val ? "✔" : "";
                        idx++;
                    }
                }, DispatcherPriority.DataBind, CancellationToken.None, TimeOut);
            } catch (TimeoutException) {
                ;//Nothing
            }
        }
    }
}
