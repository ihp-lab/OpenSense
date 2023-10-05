#nullable enable

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Runtime.CompilerServices;
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
        }

        #region Pipeline Event Handlers
        private void OnPipelineRun(object? sender, PipelineRunEventArgs e) {
            Debug.Assert(states is not null);
            Debug.Assert(!states.IsStarted);
            states.IsStarted = true;
            InfoPanelControl.TextBlockRunning.Text = "✓";
        }

        private void OnPipelineExceptionNotHandled(object? sender, PipelineExceptionNotHandledEventArgs e) {
            Debug.Assert(states?.IsRunning == true);
            states.IsStopped = true;
            InfoPanelControl.TextBlockRunning.Text = "×";
            Dispatcher.Invoke(() => {
                MessageBox.Show(e.Exception.ToString(), "Pipeline Runtime Exception", MessageBoxButton.OK, MessageBoxImage.Error);
            });
        }

        private void OnPipelineCompleted(object? sender, PipelineCompletedEventArgs e) {
            if (states?.IsStarted == false) {
                return;//Pipeline is not run, but is disposed.
            }
            Debug.Assert(states?.IsRunning == true);
            states.IsStopped = true;
            InfoPanelControl.TextBlockRunning.Text = "×";
            Dispatcher.Invoke(() => {
                MessageBox.Show($"Pipeline Completed.", "Pipeline Completed", MessageBoxButton.OK, MessageBoxImage.Information);
                //TODO: show details in PipelineCompletedEventArgs
            });
        }
        #endregion

        #region Control Event Handlers
        private void ButtonNew_Click(object? sender, RoutedEventArgs e) {
            if (states?.IsRunning == true) {
                MessageBox.Show("Pipeline is running.", "Create New Pipeline", MessageBoxButton.OK, MessageBoxImage.Hand);
                return;
            }
            VisualizerContainerControl.Clear();
            states?.Environment.Dispose();
            states = null;
            configuration = new PipelineConfiguration();
        }

        private void ButtonLoad_Click(object? sender, RoutedEventArgs e) {
            if (states?.IsRunning == true) {
                MessageBox.Show("Pipeline is running.", "Load Pipeline", MessageBoxButton.OK, MessageBoxImage.Hand);
                return;
            }
            VisualizerContainerControl.Clear();
            states?.Environment?.Dispose();
            states = null;
            var openFileDialog = new Microsoft.Win32.OpenFileDialog {
                CheckFileExists = true,
                AddExtension = true,
                DefaultExt = "*.pipe.json",
                Filter = "OpenSense Pipeline | *.pipe.json",
            };
            if (openFileDialog.ShowDialog() == true) {
                var json = File.ReadAllText(openFileDialog.FileName);
                configuration = new PipelineConfiguration(json);
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

        private void ButtonInstantiate_Click(object sender, RoutedEventArgs e) {
            if (states is not null) {
                if (states.IsStarted && !states.IsStopped) {
                    MessageBox.Show("Pipeline is running.", "Instantiate Pipeline", MessageBoxButton.OK, MessageBoxImage.Hand);
                    return;
                }
            }
            _ = ReinstantiateAndSetupControls();
        }

        private void ButtonResetControls_Click(object sender, RoutedEventArgs e) {
            if (states is null) {
                return;
            }
            VisualizerContainerControl.Setup(states.Environment);
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

        private void ButtonRun_Click(object? sender, RoutedEventArgs e) {
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
            if (ReinstantiateAndSetupControls()) {
                _ = states.Environment.Pipeline.RunAsync();
            }
        }

        private void ButtonStop_Click(object? sender, RoutedEventArgs e) {
            if (states?.IsRunning != true) {
                MessageBox.Show("Pipeline is not running.", "Stop Pipeline", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }
            VisualizerContainerControl.Clear();
            states?.Environment.Dispose();//Note: the Stop() method is not public, so we can only stop the pipeline by disposing it.
            states = null;
        }

        private void Window_Unloaded(object? sender, RoutedEventArgs e) {
            VisualizerContainerControl.Clear();
            states?.Environment.Dispose();
            states = null;
        }
        #endregion

        #region Helpers

        private static PipelineEnvironment CreateEnvironment(PipelineConfiguration configuration) {
            var services = new ServiceCollection();
            var providers = new ILoggerProvider[] {
                new SerilogLoggerProvider(),
            };
            services.AddSingleton<ILoggerFactory, LoggerFactory>(sp => new LoggerFactory(providers));
            var serviceProvider = services.BuildServiceProvider();
            var result = new PipelineEnvironment(configuration, serviceProvider);
            return result;
        }

        private void ConfigureEnvironment(in PipelineEnvironment environment) {
            environment.Pipeline.PipelineRun += OnPipelineRun;
            environment.Pipeline.PipelineExceptionNotHandled += OnPipelineExceptionNotHandled;
            environment.Pipeline.PipelineCompleted += OnPipelineCompleted;
        }

        [MemberNotNullWhen(true, nameof(states))]
        private bool ReinstantiateAndSetupControls() {
            VisualizerContainerControl.Clear();
            states?.Environment.Dispose();
            states = null;

            PipelineEnvironment env;
            try {
                env = CreateEnvironment(configuration);
            } catch (Exception ex) {
                MessageBox.Show(ex.ToString(), "Pipeline Instantiation Exception", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
            ConfigureEnvironment(env);
            VisualizerContainerControl.Setup(env);
            states = new States(env);
            return true;
        }
        #endregion

        #region INotifyPropertyChanged
        public event PropertyChangedEventHandler? PropertyChanged;

        private void SetProperty<T>(ref T field, T value, [CallerMemberName] string propertyName = null) {
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
