#nullable enable

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Psi;
using OpenSense.Pipeline;
using OpenSense.WPF.Pipeline;
using Serilog.Extensions.Logging;

namespace OpenSense.WPF.Views.Runners {
    public sealed partial class RunnerWindow : Window, INotifyPropertyChanged {

        private PipelineConfiguration configuration;

        private States? states;

        public RunnerWindow(PipelineConfiguration pipelineConfiguration) {
            InitializeComponent();

            configuration = pipelineConfiguration;
            DataContext = configuration;
        }

        #region Pipeline Event Handlers
        private void OnPipelineRun(object? sender, PipelineRunEventArgs e) {
            Debug.Assert(states is not null);
            Debug.Assert(!states.IsStarted);
            states.IsStarted = true;
            _ = Dispatcher.InvokeAsync(() => {
                InfoPanelControl.TextBlockRunning.Text = "✓";
            });
        }

        private void OnPipelineExceptionNotHandled(object? sender, PipelineExceptionNotHandledEventArgs e) {
            Debug.Assert(states?.IsRunning == true);
            states.IsStopped = true;
            _ = Dispatcher.InvokeAsync(() => {
                InfoPanelControl.TextBlockRunning.Text = "×";
                ExceptionMessageWindow.Show(this, e.Exception, "Pipeline Runtime Exception", "There is an exception while running the pipeline.");
            });
            //OnPipelineCompleted will be called after this event handler.
        }

        private void OnPipelineCompleted(object? sender, PipelineCompletedEventArgs e) {
            if (states?.IsStarted == false) {
                return;//Pipeline was not run, but is being disposed.
            }
            if (states?.IsStopped == true) {
                return;//There was an exception, we do not show message boxes again.;
            }
            Debug.Assert(states?.IsRunning == true);
            states.IsStopped = true;
            _ = Dispatcher.InvokeAsync(() => {
                InfoPanelControl.TextBlockRunning.Text = "×";
                MessageBox.Show($"Pipeline Completed.", "Pipeline Completed", MessageBoxButton.OK, MessageBoxImage.Information);
                //TODO: show details in PipelineCompletedEventArgs
            });
        }
        #endregion

        #region Control Event Handlers
        private async void ButtonNew_Click(object? sender, RoutedEventArgs e) {
            if (states?.IsRunning == true) {
                MessageBox.Show("Pipeline is running.", "Create New Pipeline", MessageBoxButton.OK, MessageBoxImage.Hand);
                return;
            }
            VisualizerContainerControl.Clear();
            await DoLongTimeOperationAsync(() => {
                states?.Environment.Dispose();
                states = null;
                configuration = new PipelineConfiguration();
            }, ui: false);
            DataContext = configuration;
        }

        private async void ButtonLoad_Click(object? sender, RoutedEventArgs e) {
            if (states?.IsRunning == true) {
                MessageBox.Show("Pipeline is running.", "Load Pipeline", MessageBoxButton.OK, MessageBoxImage.Hand);
                return;
            }
            VisualizerContainerControl.Clear();
            await DoLongTimeOperationAsync(() => {
                states?.Environment?.Dispose();
                states = null;
            }, ui: false);
            var openFileDialog = new Microsoft.Win32.OpenFileDialog {
                CheckFileExists = true,
                AddExtension = true,
                DefaultExt = "*.pipe.json",
                Filter = "OpenSense Pipeline | *.pipe.json",
            };
            if (openFileDialog.ShowDialog() == true) {
                await DoLongTimeOperationAsync(() => {
                    var json = File.ReadAllText(openFileDialog.FileName);
                    configuration = new PipelineConfiguration(json);
                }, ui: false);
                DataContext = configuration;
            }
        }

        private void ButtonEdit_Click(object? sender, RoutedEventArgs e) {
            if (states?.IsRunning == true) {
                MessageBox.Show("Pipeline is running.", "Edit Pipeline", MessageBoxButton.OK, MessageBoxImage.Hand);
                return;
            }
            var win = new PipelineEditorWindow(configuration) {
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

        private async void ButtonInstantiate_Click(object sender, RoutedEventArgs e) {
            if (states is not null) {
                if (states.IsStarted && !states.IsStopped) {
                    MessageBox.Show("Pipeline is running.", "Instantiate Pipeline", MessageBoxButton.OK, MessageBoxImage.Hand);
                    return;
                }
            }
            _ = await DoLongTimeOperationAsync(ReinstantiateAndSetupControlsAsync);
        }

        private async void ButtonResetControls_Click(object sender, RoutedEventArgs e) {
            if (states is null) {
                return;
            }
            await DoLongTimeOperationAsync(() => {
                VisualizerContainerControl.Setup(states.Environment);
            });
        }

        private void ButtonRemoveEmptyControls_Click(object sender, RoutedEventArgs e) {
            if (states is null) {
                return;
            }
            VisualizerContainerControl.RemoveEmptyControls();
        }

        private void ButtonSimplifyLayouts_Click(object sender, RoutedEventArgs e) {
            if (states is null) {
                return;
            }
            VisualizerContainerControl.SimplifyLayouts();
        }

        internal async void ButtonRun_Click(object? sender, RoutedEventArgs e) {
            if (states is not null) {
                if (states.IsStarted && !states.IsStopped) {
                    MessageBox.Show("Pipeline is running.", "Run Pipeline", MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }
                if (!states.IsStarted) {
                    _ = states.Environment.Pipeline.RunAsync();
                    return;
                }
            }
            if (await ReinstantiateAndSetupControlsAsync()) {
                Debug.Assert(states is not null);
                _ = states.Environment.Pipeline.RunAsync();
            }
        }

        private async void ButtonStop_Click(object? sender, RoutedEventArgs e) {
            if (states?.IsRunning != true) {
                MessageBox.Show("Pipeline is not running.", "Stop Pipeline", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }
            VisualizerContainerControl.Clear();
            await DoLongTimeOperationAsync(() => {
                states?.Environment.Dispose();//Note: the Stop() method is not public, so we can only stop the pipeline by disposing it.
                states = null;
            }, ui: false);
        }

        private void Window_Unloaded(object? sender, RoutedEventArgs e) {
            VisualizerContainerControl.Clear();
            states?.Environment.Dispose();
            states = null;
        }
        #endregion

        #region Helpers
        private async Task<T> DoLongTimeOperationAsync<T>(Func<T> func, bool ui = true) {
            IsEnabled = false;
            ProgressBarAction.Visibility = Visibility.Visible;
            try {
                var result = ui ? 
                    await Dispatcher.InvokeAsync(func) : await Task.Factory.StartNew(func);
                return result;
            } finally {
                ProgressBarAction.Visibility = Visibility.Hidden;
                IsEnabled = true;
            }
        }

        private async Task DoLongTimeOperationAsync(Action action, bool ui = true) {
            IsEnabled = false;
            ProgressBarAction.Visibility = Visibility.Visible;
            try {
                if (ui) {
                    await Dispatcher.InvokeAsync(action);
                } else {
                    await Task.Factory.StartNew(action);
                }
            } finally {
                ProgressBarAction.Visibility = Visibility.Hidden;
                IsEnabled = true;
            }
        }

        private static PipelineEnvironment CreateEnvironment(PipelineConfiguration configuration) {
            var services = new ServiceCollection();
            services.AddSingleton<ILoggerFactory, LoggerFactory>(sp => {
                var providers = new ILoggerProvider[] {
                    new SerilogLoggerProvider(),
                };
                var result = new LoggerFactory(providers);
                return result;
            });
            var serviceProvider = services.BuildServiceProvider();
            var result = new PipelineEnvironment(configuration, serviceProvider);
            return result;
        }

        private void ConfigureEnvironment(in PipelineEnvironment environment) {
            environment.Pipeline.PipelineRun += OnPipelineRun;
            environment.Pipeline.PipelineExceptionNotHandled += OnPipelineExceptionNotHandled;
            environment.Pipeline.PipelineCompleted += OnPipelineCompleted;
        }

        private async Task<bool> ReinstantiateAndSetupControlsAsync() {
            VisualizerContainerControl.Clear();

            try {
                await DoLongTimeOperationAsync(() => { 
                    states?.Environment.Dispose();
                    states = null;

                    var env = CreateEnvironment(configuration);
                    ConfigureEnvironment(env);
                    states = new States(env);
                }, ui: false);
            } catch (Exception ex) {
                ExceptionMessageWindow.Show(this, ex, "Pipeline Instantiation Exception", "There is an exception while instantiating the pipeline.");
                return false;
            }

            await DoLongTimeOperationAsync(() => {
                VisualizerContainerControl.Setup(states!.Environment);
            });

            return true;
        }
        #endregion

        #region INotifyPropertyChanged
        public event PropertyChangedEventHandler? PropertyChanged;

        private void SetProperty<T>(ref T field, T value, [CallerMemberName] string? propertyName = null) {
            if (!EqualityComparer<T>.Default.Equals(field, value)) {
                field = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            }
        }
        #endregion

        #region Classes
        private sealed class States {

            public PipelineEnvironment Environment { get; set; }

            public bool IsStarted { get; set; }

            public bool IsStopped { get; set; }

            public bool IsRunning => IsStarted && !IsStopped;

            public States(PipelineEnvironment environment) {
                Environment = environment;
            }
        }
        #endregion
    }
}
