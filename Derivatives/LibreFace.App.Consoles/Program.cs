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
            foreach (var filename in files) {
                var stopwatch = Stopwatch.StartNew();
                var dir = options.Output ?? Path.GetDirectoryName(filename) ?? throw new ArgumentNullException();
                using var progress = new Progress(filename);
                using var processor = new Processor(filename, dir, progress);
                processor.WaitTask.Wait();
                stopwatch.Stop();
                AnsiConsole.WriteLine($"Processed {filename} in {stopwatch.Elapsed}.");
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
All other steps are computed using the CPU, so the running time mainly depends on CPU performance.

This program is based on the Microsoft Platform for Situated Intelligence (\psi) and OpenSense.
Due to limitations of \psi, the buffer size cannot be accurately controlled,
which may result in high memory usage during runtime."
                    );
                    return HelpText.DefaultParsingErrorsHandler(result, h);
                }, e => e);
            }
            Console.WriteLine(helpText);
        }
    }
}
