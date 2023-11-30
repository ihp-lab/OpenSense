using Spectre.Console;

namespace LibreFace.App.Consoles {
    internal sealed class Progress : IProgress<TimeSpan> {

        private readonly Action<TimeSpan> _callback;

        public Progress(Action<TimeSpan> callback) {
            _callback = callback;
        }

        #region IProgress
        public void Report(TimeSpan value) {
            _callback(value);
        }
        #endregion
    }
}
