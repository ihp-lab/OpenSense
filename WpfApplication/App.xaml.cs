#nullable enable

using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using OpenSense.Pipeline;
using OpenSense.WPF.Views.Runners;
using Serilog;

namespace OpenSense.WPF {
    public partial class App : Application {

        private readonly IReadOnlyList<FileInfo> _runPipeConfigFiles;

        /// <remarks>For auto-generated Main method.</remarks>
        private App() { 
            throw new Exception("This constructor should never be called.");
        }

        public App(
            IReadOnlyList<FileInfo> runPipeConfigFiles
        ) {
            _runPipeConfigFiles = runPipeConfigFiles;
        }

        #region Application Events
        private void Application_Startup(object sender, StartupEventArgs e) {
            /* Launch Main Window */
            var mainWindow = new MainWindow();
            MainWindow = mainWindow;
            mainWindow.Show();

            /* Run Pipelines */
            if (_runPipeConfigFiles.Count > 0) {
                var runnerWindows = new List<RunnerWindow>(_runPipeConfigFiles.Count);
                foreach (var file in _runPipeConfigFiles) {
                    var json = File.ReadAllText(file.FullName);
                    var configuration = new PipelineConfiguration(json);
                    var runnerWindow = new RunnerWindow(configuration);
                    runnerWindow.Show();
                    runnerWindows.Add(runnerWindow);
                }
                var arg = new RoutedEventArgs();
                foreach (var runnerWindow in runnerWindows) {
                    runnerWindow.ButtonRun_Click(this, arg);
                }
            }
        }

        private void Application_DispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e) {
            Log.Fatal("Application unhandled exception: {exception}", e.Exception);
            MessageBox.Show(e.Exception.ToString(), "Unhandled exception", MessageBoxButton.OK, MessageBoxImage.Error);
        }

        private void Application_Exit(object sender, ExitEventArgs e) {

        } 
        #endregion
    }
}
