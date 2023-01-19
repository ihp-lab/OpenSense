using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Psi;
using OpenSense.Pipeline;
using Serilog.Extensions.Logging;

namespace OpenSense.WPF.Pipeline {
    public partial class PipelineRunnerWindow : Window, INotifyPropertyChanged {

        #region INotifyPropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;

        private void SetProperty<T>(ref T field, T value, [CallerMemberName] string propertyName = null) {
            if (!EqualityComparer<T>.Default.Equals(field, value)) {
                field = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            }
        }
        #endregion

        public PipelineConfiguration Configuration { get; private set; }
        private PipelineEnvironment Env;

        private bool running = false;

        public bool Running {
            get => running;
            private set => SetProperty(ref running, value);
        }

        public PipelineRunnerWindow(PipelineConfiguration config) {
            InitializeComponent();
            GridPipelineRuntime.DataContext = this;
            Load(config);
        }

        public PipelineRunnerWindow() : this(new PipelineConfiguration()) { }

        private void HandlePipelineException(object sender, PipelineExceptionNotHandledEventArgs e) {
            Dispatcher.Invoke(() => {
                Running = false;
                Env = null;//Stop();
            }, DispatcherPriority.Normal, CancellationToken.None, TimeSpan.FromMilliseconds(500));
            Dispatcher.Invoke(() => {
                MessageBox.Show(e.Exception.ToString(), "Pipeline Runtime Exception", MessageBoxButton.OK, MessageBoxImage.Error);
            });
        }

        private void HandlePipelineCompeleted(object sender, PipelineCompletedEventArgs e) {
            Dispatcher.Invoke(() => {
                Running = false;
                Env = null;//Stop();
            }, DispatcherPriority.Normal, CancellationToken.None, TimeSpan.FromMilliseconds(500));
            Dispatcher.Invoke(() => {
                MessageBox.Show($"Pipeline stopped", "Pipeline Stopped", MessageBoxButton.OK, MessageBoxImage.Information);
            });
        }

        private void Stop() {
            try {
                Env?.Dispose();
            } catch (Exception ex) {
                MessageBox.Show(ex.ToString(), "Pipeline Termination Exception", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            Env = null;
        }

        private void Load(PipelineConfiguration config) {
            Configuration = config;
            Running = false;
            Env = null;
            Dispatcher.Invoke(() => {
                GridPipelineConfiguration.DataContext = Configuration;
                ClearControls();
            });
        }

        private static PipelineEnvironment InstantiatePipeline(PipelineConfiguration configuration) {
            var collection = new ServiceCollection();
            var providers = new ILoggerProvider[] {
                    new SerilogLoggerProvider(),//use the static serilog logger
                };
            collection.AddSingleton<ILoggerFactory, LoggerFactory>(sp => new LoggerFactory(providers));
            var serviceProvider = collection.BuildServiceProvider();
            var result = new PipelineEnvironment(configuration, serviceProvider);
            return result;
        }

        #region gen controls

        private void ClearControls() {
            ContentControlDisplay.RowDefinitions.Clear();
            ContentControlDisplay.ColumnDefinitions.Clear();
            ContentControlDisplay.Children.Clear();
        }

        private void AllocateRowsAndColumns(int rows, int cols) {
            for (var i = 0; i < rows; i++) {
                ContentControlDisplay.RowDefinitions.Add(new RowDefinition());
            }
            var rowStyle = (Style)ContentControlDisplay.Resources["styleSplitterRow"];
            Debug.Assert(rowStyle is not null);
            for (var i = 0; i < rows - 1; i++) {
                var splitter = new GridSplitter() { 
                    Style = rowStyle,
                    Tag = i,
                };
                Grid.SetRow(splitter, i);
                Grid.SetColumnSpan(splitter, cols);
                ContentControlDisplay.Children.Add(splitter);
            }
            for (var i = 0; i < cols; i++) {
                ContentControlDisplay.ColumnDefinitions.Add(new ColumnDefinition());
            }
            var colStyle = (Style)ContentControlDisplay.Resources["styleSplitterCol"];
            Debug.Assert(colStyle is not null);
            for (var i = 0; i < cols - 1; i++) {
                var splitter = new GridSplitter() { 
                    Style = colStyle,
                    Tag = i,
                };
                Grid.SetColumn(splitter, i);
                Grid.SetRowSpan(splitter, rows);
                ContentControlDisplay.Children.Add(splitter);
            }
        }

        private void AddControl(UIElement control, int row, int col) {
            Grid.SetRow(control, row);
            Grid.SetColumn(control, col);
            ContentControlDisplay.Children.Add(control);
        }

        private void GenerateControls() {
            ClearControls();
            if (Env is null || Env.Instances.Count == 0) {
                return;
            }
            switch (ComboBoxView.SelectedIndex) {
                case 1://list
                    var scrollViewer = new ScrollViewer();
                    ContentControlDisplay.Children.Add(scrollViewer);
                    var stackPanel = new StackPanel();
                    scrollViewer.Content = stackPanel;
                    foreach (var compEnv in Env.Instances) {
                        var control = new InstanceControlCreatorManager().Create(compEnv.Instance);
                        var container = new InstanceContainerControl() { 
                            DataContext = Tuple.Create(compEnv.Configuration.Name, compEnv.Instance, control),
                        };
                        stackPanel.Children.Add(container);
                    }
                    break;
                case 0://grid
                    var graph = new PipelineConnectionGraph(Env);
                    var positions = graph.CalcPositions();
                    foreach (var compEnv in Env.Instances) {
                        var control = new InstanceControlCreatorManager().Create(compEnv.Instance);
                        var container = new GridViewCellControl() {
                            DataContext = Tuple.Create(compEnv.Configuration.Name, compEnv.Instance, control),
                        };
                        var position = positions[compEnv.Configuration.Id];
                        AddControl(container, position.Hierachy, position.Offset);
                    }
                    var numRow = positions.Values.Max(p => p.Hierachy) + 1;
                    var numCol = positions.Values.Max(p => p.Offset) + 1;
                    AllocateRowsAndColumns(numRow, numCol);//add last, for correct overlay
                    break;
            }
        }

        private void ComboBoxView_SelectionChanged(object sender, SelectionChangedEventArgs e) {
            if (Env is null || Env.Instances.Count == 0) {
                return;
            }
            GenerateControls();
        }
        #endregion

        private void ButtonNew_Click(object sender, RoutedEventArgs e) {
            if (Running) {
                MessageBox.Show("Pipeline is running");
                return;
            }
            Load(new PipelineConfiguration());
        }

        private void ButtonLoad_Click(object sender, RoutedEventArgs e) {
            if (Running) {
                MessageBox.Show("Pipeline is running");
                return;
            }
            var openFileDialog = new Microsoft.Win32.OpenFileDialog {
                CheckFileExists = true,
                AddExtension = true,
                DefaultExt = "*.pipe.json",
                Filter = "OpenSense Pipeline | *.pipe.json",
            };
            if (openFileDialog.ShowDialog() == true) {
                var json = File.ReadAllText(openFileDialog.FileName);
                var config = new PipelineConfiguration(json);//TODO: extra json converters
                Load(config);
            }
        }

        private void ButtonEdit_Click(object sender, RoutedEventArgs e) {
            if (Configuration is null) {
                MessageBox.Show("Pipeline not set");
                return;
            }
            if (Running) {
                MessageBox.Show("Pipeline is running");
                return;
            }
            var win = new PipelineEditorWindow(Configuration) { 
                Owner = Owner,
                WindowStartupLocation = WindowStartupLocation.Manual,
                Left = Left,
                Top = Top,
                Width = Width,
                Height = Height,
                WindowState = WindowState,
            };
            win.Show();
            Close();
        }

        private void ButtonRun_Click(object sender, RoutedEventArgs e) {
            if (Configuration is null) {
                MessageBox.Show("Pipeline not set");
                return;
            }
            if (Running) {
                MessageBox.Show("Pipeline is running");
                return;
            }
            PipelineEnvironment env;
            try {
                env = InstantiatePipeline(Configuration);
            } catch (Exception ex) {
                MessageBox.Show(ex.ToString(), "Pipeline Instantiation Exception", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            Env = env;
            GenerateControls();
            Env.Pipeline.PipelineExceptionNotHandled += HandlePipelineException;
            Env.Pipeline.PipelineCompleted += HandlePipelineCompeleted;
            Env.Pipeline.RunAsync();
            Running = true;
        }

        private void ButtonStop_Click(object sender, RoutedEventArgs e) {
            if (Configuration is null) {
                MessageBox.Show("Pipeline not set");
                return;
            }
            if (!Running) {
                MessageBox.Show("Pipeline is not running");
                return;
            }
            Stop();
            Load(Configuration);
        }

        private void Window_Unloaded(object sender, RoutedEventArgs e) {
            Stop();
        }
    }
}
