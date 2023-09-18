using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using CommandLine;
using Mediapipe.Net.Framework.Protobuf;
using Microsoft.Psi;
using Microsoft.Psi.Imaging;
using OpenSense.Components.LibreFace;
using OpenSense.Components.MediaPipe.NET;
using OpenSense.Components.OpenFace;

namespace LibreFace.Benchmarks2 {
    internal static class Program {

        private static List<Shared<Image>> images = new();

        public static void Main(string[] args) {
            Parser.Default.ParseArguments<Options>(args)
               .WithParsed(RunOptions)
               .WithNotParsed(HandleParseError);
        }

        private static void RunOptions(Options opts) {
            Console.WriteLine("Preparing images");
            PrepareImages(opts.ImageDirectory, opts.MaxImageCount);

            var openFace = RunTest(CreateOpenFacePipeline, "OpenFace", opts.WarmupLoops, opts.TestLoops, opts.ThreadCount);
            var libreFace = RunTest(CreateLibreFacePipeline, "LibreFace", opts.WarmupLoops, opts.TestLoops, opts.ThreadCount);

            Console.WriteLine(openFace);
            Console.WriteLine(libreFace);

            Console.WriteLine("All Completed");
        }
        private static void HandleParseError(IEnumerable<Error> errs) {
            //handle errors
        }

        private static void PrepareImages(string dir, int max) {
            foreach (var filename in Directory.EnumerateFiles(dir).Take(max)) {
                var bitmap = new System.Drawing.Bitmap(filename);
                var image = ImagePool.GetOrCreateFromBitmap(bitmap);
                images.Add(image);
            }
            Console.WriteLine($"Loaded {images.Count} images");
        }

        private static Pipeline CreateOpenFacePipeline(int threadCount) {
            var pipeline = Pipeline.Create(
                "OpenFace",
                DeliveryPolicy.Unlimited,
                threadCount: threadCount
            );

            var generator = Generators.Sequence(pipeline, images, TimeSpan.FromTicks(1));

            var openFace = new OpenFace(pipeline);
            generator.PipeTo(openFace);

#if DEBUG
            openFace.Do(v => Console.WriteLine("OpenFace output +1"));
#endif

            return pipeline;
        }

        private static Pipeline CreateLibreFacePipeline(int threadCount) {
            var pipeline = Pipeline.Create(
                "LibreFace",
                DeliveryPolicy.Unlimited,
                threadCount: threadCount
            );

            var generator = Generators.Sequence(pipeline, images, TimeSpan.FromTicks(1));

            var config = new MediaPipeConfiguration();
            var wrapper = new SolutionWrapper(pipeline, config.InputSidePackets, config.InputStreams, config.OutputStreams, config.Graph, null);
            dynamic consumer = wrapper.Inputs["image"];
            Microsoft.Psi.Operators.PipeTo(generator, consumer);

            var detector = new LibreFaceDetector(pipeline);
            generator.PipeTo(detector.ImageIn);
            var producer = (Emitter<List<NormalizedLandmarkList>>)wrapper.Outputs["multi_face_landmarks"];
            var producerConverted = producer.Select(l => (IReadOnlyList<NormalizedLandmarkList>)l);
            Microsoft.Psi.Operators.PipeTo(producerConverted, detector.DataIn);

#if DEBUG
            detector.ActionUnitIntensityOut.Do(v => Console.WriteLine("LibreFace outptu +1"));
#endif

            return pipeline;
        }

        private static TimeSpan RunPipeline(Pipeline pipeline) {
            var stopWatch = new Stopwatch();
            stopWatch.Start();
            pipeline.Run();
            stopWatch.Stop();
            return stopWatch.Elapsed;
        }

        private static Report RunTest(Func<int, Pipeline> creator, string name, int warmup, int test, int threadCount) {
            Console.WriteLine($"{name}");
            for (var i = 0; i < warmup; i++) {
                Console.WriteLine($"Warm up {name} {i + 1}");
                using var pipeline = creator(threadCount);
                _ = RunPipeline(pipeline);
            }
            var times = new TimeSpan[test];
            for (var i = 0; i < test; i++) {
                Console.WriteLine($"Run {name} {i + 1}");
                using var pipeline = creator(threadCount);
                var time = RunPipeline(pipeline);
                times[i] = time;
            }
            var avg = TimeSpan.FromTicks(times.Select(t => t.Ticks).Sum() / test);
            var std = TimeSpan.FromTicks((long)times.Select(t => t.Ticks).Std());
            var result = new Report() {
                Name = name,
                Images = images.Count,
                Avg = avg,
                Std = std,
            };
            return result;
        }
    }
}
