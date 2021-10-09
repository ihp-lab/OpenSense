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

namespace OpenSense.Wpf.Pipeline {
    public partial class PipelineExecuterWindow : Window, INotifyPropertyChanged {

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

        public PipelineExecuterWindow(PipelineConfiguration config) {
            InitializeComponent();
            GridPipelineRuntime.DataContext = this;
            Load(config);
        }

        public PipelineExecuterWindow() : this(new PipelineConfiguration()) { }

        private void HandlePipelineException(object sender, PipelineExceptionNotHandledEventArgs e) {
            Dispatcher.Invoke(() => {
                Running = false;
                Env = null;//Stop();
            }, DispatcherPriority.Normal, CancellationToken.None, TimeSpan.FromMilliseconds(500));
            Dispatcher.Invoke(() => {
                MessageBox.Show(e.Exception.ToString(), "Pipeline runtime exception", MessageBoxButton.OK, MessageBoxImage.Error);
            });
        }

        private void HandlePipelineCompeleted(object sender, PipelineCompletedEventArgs e) {
            Dispatcher.Invoke(() => {
                Running = false;
                Env = null;//Stop();
            }, DispatcherPriority.Normal, CancellationToken.None, TimeSpan.FromMilliseconds(500));
            Dispatcher.Invoke(() => {
                MessageBox.Show("Pipeline stopped", "Pipeline stopped", MessageBoxButton.OK, MessageBoxImage.Information);
            });
        }

        private void Stop() {
            try {
                Env?.Dispose();
            } catch (Exception ex) {
                Debug.WriteLine(ex.ToString());
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

        private void Instantiate() {
            try {
                var collection = new ServiceCollection();
                collection.AddSingleton<ILoggerProvider, SerilogLoggerProvider>();//use the static serilog logger
                var serviceProvider = collection.BuildServiceProvider();
                Env = new PipelineEnvironment(Configuration, serviceProvider);
                GenerateControls();
            } catch (Exception ex) {
                Stop();
                MessageBox.Show(ex.ToString(), "Pipeline instantiation exception", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        #region gen controls

        private void ClearControls() {
            ContentControlDisplay.RowDefinitions.Clear();
            ContentControlDisplay.ColumnDefinitions.Clear();
            ContentControlDisplay.Children.Clear();
        }

        private const int SPLITTER_THICHNESS = 2;

        private void AllocateRowsAndColumns(int rows, int cols) {
            for (var i = 0; i < rows; i++) {
                ContentControlDisplay.RowDefinitions.Add(new RowDefinition());
            }
            for (var i = 0; i < rows - 1; i++) {
                var splitter = new GridSplitter();
                splitter.VerticalAlignment = VerticalAlignment.Bottom;
                splitter.HorizontalAlignment = HorizontalAlignment.Stretch;
                splitter.Height = SPLITTER_THICHNESS;
                Grid.SetRow(splitter, i);
                Grid.SetColumnSpan(splitter, cols);
                ContentControlDisplay.Children.Add(splitter);
            }
            for (var i = 0; i < cols; i++) {
                ContentControlDisplay.ColumnDefinitions.Add(new ColumnDefinition());
            }
            for (var i = 0; i < cols - 1; i++) {
                var splitter = new GridSplitter();
                splitter.VerticalAlignment = VerticalAlignment.Stretch;
                splitter.HorizontalAlignment = HorizontalAlignment.Right;
                splitter.Width = SPLITTER_THICHNESS;
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
                        var container = new InstanceContainerControl(compEnv.Configuration.Name, control);
                        stackPanel.Children.Add(container);
                    }
                    break;
                case 0://grid
                    var graph = new PipelineConnectionGraph(Env);
                    var positions = graph.CalcPositions();
                    foreach (var compEnv in Env.Instances) {
                        var control = new InstanceControlCreatorManager().Create(compEnv.Instance);
                        var container = new InstanceContainerControl(compEnv.Configuration.Name, control);
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
            var win = new PipelineEditorWindow(Configuration);
            win.ShowDialog();
            Load(Configuration);
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
            Instantiate();
            if (Env is null) {
                return;
            }
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
            Env?.Pipeline.Dispose();
        }
    }
}
