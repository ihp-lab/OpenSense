using Spectre.Console;

namespace LibreFace.App.Consoles {
    internal sealed class BarChartUI : IConsoleUI {

        private static readonly Color PendingColor = Color.Gold1;

        private static readonly Color ProcessingColor = Color.DarkOrange;

        private static readonly Color DoneColor = Color.Aqua;

        private static readonly Color ErrorColor = Color.Red;

        private readonly BarChart _barChart = new BarChart();

        private readonly TaskCompletionSource _tcs = new();

        private readonly object _lock = new();

        private LiveDisplayContext? context;

        private int pendingCount;

        private int processingCount;

        private int doneCount;

        private int errorCount;

        #region IConsoleUI
        public void Initialize(IReadOnlyList<string> files) {

            pendingCount = files.Count;
            _barChart
                .WithMaxValue(files.Count)
                .Label("Status")
                .AddItem("Pending", pendingCount, PendingColor)
                .AddItem("Processing", 0, ProcessingColor)
                .AddItem("Done", 0, DoneColor)
                .AddItem("Error", 0, ErrorColor)
                ;

            AnsiConsole.Live(_barChart).StartAsync(ctx => {
                context = ctx;
                return _tcs.Task;
            });

            context?.Refresh();
        }

        public void SetState(int index, State state) {
            lock (_lock) {
                switch (state) {
                    case State.Processing:
                        pendingCount--;
                        processingCount++;
                        _barChart.Data[0] = new BarChartItem("Pending", pendingCount, PendingColor);
                        _barChart.Data[1] = new BarChartItem("Processing", processingCount, ProcessingColor);
                        break;
                    case State.Done:
                        processingCount--;
                        doneCount++;
                        _barChart.Data[1] = new BarChartItem("Processing", processingCount, ProcessingColor);
                        _barChart.Data[2] = new BarChartItem("Done", doneCount, DoneColor);
                        break;
                    case State.Error:
                        processingCount--;
                        errorCount++;
                        _barChart.Data[1] = new BarChartItem("Processing", processingCount, ProcessingColor);
                        _barChart.Data[3] = new BarChartItem("Error", errorCount, ErrorColor);
                        break;
                }
                context?.Refresh();
            }
        }

        public void SetElapsed(int index, TimeSpan elapsed) {
            //no-op
        }

        public void SetProcessed(int index, TimeSpan processed) {
            //no-op
        }
        #endregion

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
