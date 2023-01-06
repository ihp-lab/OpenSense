using System;
using System.ComponentModel;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using OpenSense.Components.OpenSmile.Visualizer;

namespace OpenSense.WPF.Components.OpenSmile.Visualizer {
    public partial class OpenSmileVisualizerInstanceControl : UserControl {

        private OpenSmileVisualizer Comp => DataContext as OpenSmileVisualizer;

        public OpenSmileVisualizerInstanceControl() {
            InitializeComponent();
        }

        private void GridOpenSmileVisulization_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e) {
            OpenSmileFeatureDisplayGrid.DataContext = OpenSmileFeatureDisplay;
            if (Comp != null) {
                Comp.PropertyChanged += OpenSmileVectorOutputPropertyChangedEventHandler;
            }
        }

        private void OpenSmileVectorOutputPropertyChangedEventHandler(object sender, PropertyChangedEventArgs e) {
            if (e.PropertyName == nameof(OpenSmileVisualizer.Vector)) {
                Dispatcher.Invoke(() => {
                    var v = Comp.Vector;
                    if (v != null) {
                        if (OpenSmileFeatureList.Items.Count != v.Fields.Count) {
                            OpenSmileFeatureList.ItemsSource = v.Fields;
                        }

                        var i = OpenSmileShowFeatureIndex;
                        if (0 <= i && i < v.Fields.Count) {
                            foreach (var d in v.Fields[i].Data) {
                                OpenSmileFeatureDisplay.Update(d);
                            }
                        }
                    }
                }, DispatcherPriority.Normal, CancellationToken.None, TimeSpan.FromMilliseconds(500));
            }
        }

        private void OpenSmileFeatureList_SelectionChanged(object sender, SelectionChangedEventArgs e) {
            OpenSmileShowFeatureIndex = OpenSmileFeatureList.SelectedIndex;
            lock (OpenSmileFeatureDisplay) {
                OpenSmileFeatureDisplay.Clear();
            }
        }

        private int OpenSmileShowFeatureIndex = 0;
        private DisplayFloat OpenSmileFeatureDisplay { get; set; } = new DisplayFloat();
    }
}
