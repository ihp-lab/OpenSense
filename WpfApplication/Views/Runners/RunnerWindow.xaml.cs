#nullable enable

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
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
            InfoPanelControl.TextBlockRunning.Text = "True";
        }

        private void OnPipelineExceptionNotHandled(object? sender, PipelineExceptionNotHandledEventArgs e) {
            Debug.Assert(states?.IsRunning == true);
            states.IsStopped = true;
            InfoPanelControl.TextBlockRunning.Text = "False";
            Dispatcher.Invoke(() => {
                MessageBox.Show(e.Exception.ToString(), "Pipeline Runtime Exception", MessageBoxButton.OK, MessageBoxImage.Error);
            });
        }

        private void OnPipelineCompleted(object? sender, PipelineCompletedEventArgs e) {
            Debug.Assert(states?.IsRunning == true);
            states.IsStopped = true;
            InfoPanelControl.TextBlockRunning.Text = "False";
            Dispatcher.Invoke(() => {
                MessageBox.Show($"Pipeline Completed.", "Pipeline Completed", MessageBoxButton.OK, MessageBoxImage.Information);
                //TODO: show details in PipelineCompletedEventArgs
            });
        }
        #endregion

        #region Control Event Handlers
        private void ButtonNew_Click(object? sender, RoutedEventArgs e) {
            if (states?.IsRunning == true) {
                MessageBox.Show("Pipeline is running.");
                return;
            }
            states?.Environment.Dispose();
            states = null;
            configuration = new PipelineConfiguration();
        }

        private void ButtonLoad_Click(object? sender, RoutedEventArgs e) {
            if (states?.IsRunning == true) {
                MessageBox.Show("Pipeline is running.");
                return;
            }
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
                MessageBox.Show("Pipeline is running.");
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

        private void ButtonRun_Click(object? sender, RoutedEventArgs e) {
            if (states is not null) {
                if (states.IsStarted && !states.IsStopped) {
                    MessageBox.Show("Pipeline is running.");
                    return;
                }
                states.Environment.Dispose();
                states = null;
            }
            PipelineEnvironment env;
            try {
                env = CreateEnvironment(configuration);
            } catch (Exception ex) {
                MessageBox.Show(ex.ToString(), "Failed to instantiate the pipeline.");
                return;
            }
            ConfigureEnvironment(env);
            states = new States(env);
            VisualizerContainerControl.Setup(env);
            _ = env.Pipeline.RunAsync();
        }

        private void ButtonStop_Click(object? sender, RoutedEventArgs e) {
            if (states?.IsRunning != true) {
                MessageBox.Show("Pipeline is not running.");
                return;
            }
            states?.Environment.Dispose();//Note: the Stop() method is not public, so we can only stop the pipeline by disposing it.
            states = null;
        }

        private void ButtonDumpLayout_Click(object sender, RoutedEventArgs e) {
            VisualizerContainerControl.ConsoleDump();
        }

        private void Window_Unloaded(object? sender, RoutedEventArgs e) {
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
