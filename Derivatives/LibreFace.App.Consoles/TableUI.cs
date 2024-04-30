using Spectre.Console;

namespace LibreFace.App.Consoles {
    internal sealed class TableUI : IConsoleUI, IDisposable {

        private static readonly TimeSpan RefreshInterval = TimeSpan.FromMilliseconds(100);

        private readonly Table _table = new Table();

        private readonly TaskCompletionSource _tcs = new();

        private readonly object _lock = new();

        private LiveDisplayContext? context;

        private DateTime lastRefresh;

        #region IConsoleUI
        public void Initialize(IReadOnlyList<string> files) {
            _table
                .AddColumn("#")
                .AddColumn("File")
                .AddColumn("Spent")
                .AddColumn("Processed")
                .AddColumn("Status")
                ;

            AnsiConsole.Live(_table).StartAsync(ctx => {
                context = ctx;
                return _tcs.Task;
            });

            for (var i = 0; i < files.Count; i++) {
                var filename = files[i];
                _table.AddRow($"{i + 1}", filename, FormatTime(TimeSpan.Zero), FormatTime(TimeSpan.Zero), "Pending");
            }

            context?.Refresh();
            lastRefresh = DateTime.UtcNow;
        }

        public void SetState(int index, State state) {
            _table.UpdateCell(index, 4, state.ToString());
            Refresh();
        }

        public void SetElapsed(int index, TimeSpan elapsed) {
            _table.UpdateCell(index, 2, FormatTime(elapsed));
            Refresh();
        }

        public void SetProcessed(int index, TimeSpan processed) {
            _table.UpdateCell(index, 3, FormatTime(processed));
            Refresh();
        }
        #endregion

        private void Refresh() {
            var now = DateTime.UtcNow;
            if (now - lastRefresh < RefreshInterval) {
                return;
            }
            lock (_lock) {
                if (now - lastRefresh < RefreshInterval) {
                    return;
                }
                context?.Refresh();
                lastRefresh = now;
            }
            
        }

        private static string FormatTime(TimeSpan time) {
            return $"{time:d\\.hh\\:mm\\:ss\\.f}";
        }

        #region IDisposable
        private bool disposed;

        public void Dispose() {
            if (disposed) {
                return;
            }
            disposed = true;

            _tcs.SetResult();
        }
        #endregion
    }
}
