using System.Diagnostics;
using CommandLine;
using CommandLine.Text;
using Spectre.Console;

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
            var files = options.File.ToArray();
            if (files.Length == 0) {
                return;
            }
            var source = new TaskCompletionSource();
            var table = new Table()
                .AddColumn("File")
                .AddColumn("Spent")
                .AddColumn("Processed")
                ;
            LiveDisplayContext? context = null;
            AnsiConsole.Live(table).StartAsync(ctx => {
                context = ctx;
                return source.Task;
            });
            for (var i = 0; i < files.Length; i++) {
                var filename = files[i];
                table.AddRow(filename, FormatTime(TimeSpan.Zero), FormatTime(TimeSpan.Zero));
                var stopwatch = Stopwatch.StartNew();
                var dir = options.Output ?? Path.GetDirectoryName(filename) ?? throw new ArgumentNullException();
                var last = 0L;
                var progress = new Progress(v => {
                    if (stopwatch.ElapsedMilliseconds - last < 100) {
                        return;
                    }
                    table.UpdateCell(i, 1, FormatTime(stopwatch.Elapsed));
                    table.UpdateCell(i, 2, FormatTime(v));
                    context?.Refresh();
                    last = stopwatch.ElapsedMilliseconds;
                });
                using var processor = new Processor(filename, dir, options.NumFaces, progress);
                processor.Run();
                stopwatch.Stop();
            }
            source.SetResult();
        }

        private static string FormatTime(TimeSpan time) {
            return $"{time:d\\.hh\\:mm\\:ss\\.f}";
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
