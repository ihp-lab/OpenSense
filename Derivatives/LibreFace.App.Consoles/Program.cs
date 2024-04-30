using System.Collections.Concurrent;
using System.Diagnostics;
using CommandLine;
using CommandLine.Text;

namespace LibreFace.App.Consoles {
    internal static class Program {

        public static void Main(string[] args) {
            var parser = new Parser(with => with.HelpWriter = null);

            var parserResult = parser.ParseArguments<Options>(args);
            parserResult
                .WithParsed(Run)
                .WithNotParsed(errs => DisplayHelp(parserResult, errs))
                ;
        }

        private static void Run(Options options) {
            var files = options.Paths
                .SelectMany(n => { 
                    var attr = File.GetAttributes(n);
                    if (attr.HasFlag(FileAttributes.Directory)) {
                        return Directory.GetFiles(n, "*", SearchOption.AllDirectories);
                    } else {
                        return [Path.GetFullPath(n)];
                    }
                })
                .Distinct()
                .ToArray();
            if (files.Length == 0) {
                return;
            }
            IConsoleUI ui = options.Succinct ?
                new BarChartUI() : new TableUI();
            try {
                using var model = new ModelContext(options.DeviceId);
                var items = new ConcurrentBag<int>(Enumerable.Range(0, files.Length));
                var tasks = new Task[options.Concurrency];
                ui.Initialize(files);
                for (var t = 0; t < options.Concurrency; t++) {
                    tasks[t] = Task.Run(() => {
                        while (items.TryTake(out var i)) {
                            var filename = files[i];
                            var finalLength = TimeSpan.Zero;
                            var stopwatch = Stopwatch.StartNew();
                            var dir = options.Output ?? Path.GetDirectoryName(filename) ?? throw new ArgumentNullException();
                            var progress = new Progress(v => {
                                finalLength = v;
                                ui.SetElapsed(i, stopwatch.Elapsed);
                                ui.SetProcessed(i, v);
                            });
                            ui.SetState(i, State.Processing);
                            try {
                                using var processor = new Processor(filename, dir, options.NumFaces, model, progress);
                                processor.Run();
                                ui.SetProcessed(i, finalLength);
                                ui.SetState(i, State.Done);
                            } catch (Exception) {
                                ui.SetState(i, State.Error);
                            }
                            ui.SetElapsed(i, stopwatch.Elapsed);
                            stopwatch.Stop();
                        }
                    });
                }
                Task.WaitAll(tasks);
            } finally {
                if (ui is IDisposable disposable) {
                    disposable.Dispose();
                }
            }
        }

        private static void DisplayHelp<T>(ParserResult<T> result, IEnumerable<Error> errs) {
            HelpText helpText;
            if (errs.IsVersion()) {//check if error is version request
                helpText = HelpText.AutoBuild(result);
            } else {
                helpText = HelpText.AutoBuild(result, h => {
                    h.AdditionalNewLineAfterOption = true;
                    h.AddNewLineBetweenHelpSections = true;
                    h.MaximumDisplayWidth = int.MaxValue;
                    h.AddPreOptionsText(
@"This is the LibreFace command line application.

It takes a video file as input and outputs a JSON file containing the detection results.
The processing pipeline is as follows:
1 - Read frames from the video file. (FFMpeg)
2 - For each frame, run facial landmark detection using MediaPipe. (MediaPipe.NET)
3 - Perform image alignment on the original image using the landmark detection results to 
    obtain a 224*224 RGB 24bpp input image for each face. (OpenCVSharp)
4 - Input each aligned face image from each frame into LibreFace to obtain detection results for 
    Action Unit Intensity, Action Unit Presence, and Facial Expression. (ONNX)
5 - Write the results to the output file.

Please install CUDA before using, as LibreFace utilizes CUDA for acceleration.
All other steps are computed using the CPU, so the running time mainly depends on CPU performance."
                    );
                    return HelpText.DefaultParsingErrorsHandler(result, h);
                }, e => e);
            }
            Console.WriteLine(helpText);
        }
    }
}
