using Spectre.Console;

namespace LibreFace.App.Consoles {
    internal sealed class Progress : IProgress<double>, IDisposable {

        private readonly string _filename;

        private ProgressTask? task;

        public Progress(string filename) {
            _filename = filename;
            AnsiConsole.Progress().Start(OnStart);
        }

        private void OnStart(ProgressContext context) {
            task = context.AddTask(_filename, maxValue: 1);
        }

        #region IProgress
        public void Report(double value) { 
            if (task is null) {
                return;
            }
            task.Value = value;
        }
        #endregion

        #region IDisposable
        private bool disposed;

        public void Dispose() {
            if (disposed) {
                return;
            }
            disposed = true;

            task?.StopTask();
        }
        #endregion
    }
}
