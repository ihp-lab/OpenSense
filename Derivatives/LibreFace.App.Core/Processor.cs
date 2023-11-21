using Mediapipe.Net.Framework.Protobuf;
using Microsoft.Psi;
using Microsoft.Psi.Imaging;
using OpenSense.Components;
using OpenSense.Components.Builtin;
using OpenSense.Components.CollectionOperators;
using OpenSense.Components.FFMpeg;
using OpenSense.Components.LibreFace;
using OpenSense.Components.MediaPipe.NET;
using OpenSense.Components.Psi.Imaging;
using OpenSense.Pipeline;

namespace LibreFace.App {
    public sealed class Processor : IDisposable {

        private static readonly DeliveryPolicy DeliveryPolicy = DeliveryPolicy.Unlimited;

        private readonly PipelineEnvironment _env;

        private readonly TaskCompletionSource _tcs = new TaskCompletionSource();

        public Task WaitTask => _tcs.Task;

        public Processor(string filename, string outDir, IProgress<double> progress) {
            var config = CreatePipelineConfiguration(filename, outDir);
            _env = new PipelineEnvironment(config, serviceProvider: null);

            var pipe = _env.Pipeline;
            var reader = (FileSource)_env.Instances.Single(i => i.Configuration.GetType() == typeof(FileSourceConfiguration)).Instance;
            var mediapipeEnv = _env.Instances.Single(i => i.Configuration.GetType() == typeof(MediaPipeConfiguration));

            var injector = new DefaultValueInjector<IReadOnlyList<NormalizedLandmarkList>>(pipe) { 
                InputAbsenceTolerance = TimeSpan.MaxValue,
                ReferenceAbsenceTolerance = TimeSpan.MaxValue,
                StoppingTimeout = TimeSpan.FromMilliseconds(1),
            };
            var mediapipeOutputPort = new PortConfiguration() { 
                Identifier = "multi_face_landmarks",
            };
            var mediapipeOutput = mediapipeEnv
                .Configuration
                .GetMetadata()
                .GetProducer<IReadOnlyList<NormalizedLandmarkList>>(mediapipeEnv.Instance, mediapipeOutputPort)
                ;
            mediapipeOutput.PipeTo(injector);
            reader.Select(i => (object?)i).PipeTo(injector.ReferenceIn);
            var replacer = new NullToEmptyReplacer<NormalizedLandmarkList, IReadOnlyList<NormalizedLandmarkList>>(pipe) {
            };
            injector.PipeTo(replacer);
            var libreface = new LibreFaceDetector(pipe, DeliveryPolicy) {
            };
            reader.PipeTo(libreface.ImageIn);
            replacer.PipeTo(libreface.DataIn);
            var combined = libreface
                .ActionUnitPresenceOut
                .Join(libreface.ActionUnitIntensityOut)
                .Join(libreface.FacialExpressionOut);
            var stem = Path.GetFileNameWithoutExtension(filename);
            var outFilename = Path.Combine(outDir, stem + ".json");
            var writer = new LibreFaceJsonWriter(pipe, outFilename) {
            };
            combined.PipeTo(writer);

            pipe.ProgressReportInterval = TimeSpan.FromSeconds(0.5);
            pipe.PipelineCompleted += OnPipelineCompleted;
            pipe.PipelineExceptionNotHandled += OnPipelineExceptionNotHandled;

            _ = pipe.RunAsync(ReplayDescriptor.ReplayAll, progress);
        }

        private void OnPipelineCompleted(object? sender, PipelineCompletedEventArgs args) {
            _tcs.TrySetResult();
        }

        private void OnPipelineExceptionNotHandled(object? sender, PipelineExceptionNotHandledEventArgs args) {
            _tcs.TrySetException(args.Exception);
        }

        private static PipelineConfiguration CreatePipelineConfiguration(string filename, string outDir) {
            var config = new PipelineConfiguration() { 
                DeliveryPolicy = DeliveryPolicy,
            };
            var reader = new FileSourceConfiguration() {
                Filename = filename,
                PixelFormat = PixelFormat.RGB_24bpp,
            };
            config.Instances.Add(reader);
            var converter = new PixelFormatConverterConfiguration() {
                TargetPixelFormat = PixelFormat.RGB_24bpp,//MediaPipe and LibreFace expects RGB, convert once at here to save future conversions.
                BypassIfPossible = true,
                Inputs = {
                    new InputConfiguration() {
                        LocalPort = new PortConfiguration() {
                            Identifier = nameof(PixelFormatConverter.In),
                        },
                        RemoteId = reader.Id,
                        RemotePort = new PortConfiguration() {
                            Identifier = nameof(FileSource.Out),
                        },
                    },
                },
            };
            config.Instances.Add(converter);
            var mediapipe = new MediaPipeConfiguration() {
                Inputs = { 
                    new InputConfiguration() {
                        LocalPort = new PortConfiguration() {
                            Identifier = "image",
                        },
                        RemoteId = converter.Id,
                        RemotePort = new PortConfiguration() {
                            Identifier = nameof(PixelFormatConverter.Out),
                        },
                    },
                },
            };
            config.Instances.Add(mediapipe);
            return config;
        }

        #region IDisposable
        private bool disposed;

        public void Dispose() {
            if (disposed) {
                return;
            }
            disposed = true;

            _env.Dispose();
            _tcs.TrySetCanceled();
        }
        #endregion
    }
}
