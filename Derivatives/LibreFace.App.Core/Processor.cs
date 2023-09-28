using Microsoft.Psi;
using Microsoft.Psi.Media;
using OpenSense.Components;
using OpenSense.Components.LibreFace;
using OpenSense.Components.MediaPipe.NET;
using OpenSense.Components.Psi.Data;
using OpenSense.Components.Psi.Media;
using OpenSense.Pipeline;

namespace LibreFace.App {
    public sealed class Processor : IDisposable {

        private readonly PipelineEnvironment _env;

        private readonly TaskCompletionSource _tcs = new TaskCompletionSource();

        public Task WaitTask => _tcs.Task;

        public Processor(string filename, string outDir, IProgress<double> progress) {
            var config = CreatePipelineConfiguration(filename, outDir);
            _env = new PipelineEnvironment(config, serviceProvider: null);
            _env.Pipeline.ProgressReportInterval = TimeSpan.FromSeconds(0.5);
            _env.Pipeline.PipelineCompleted += OnPipelineCompleted;
            _env.Pipeline.PipelineExceptionNotHandled += OnPipelineExceptionNotHandled;
            _ = _env.Pipeline.RunAsync(ReplayDescriptor.ReplayAll, progress);
        }

        private void OnPipelineCompleted(object? sender, PipelineCompletedEventArgs args) {
            _tcs.TrySetResult();
        }

        private void OnPipelineExceptionNotHandled(object? sender, PipelineExceptionNotHandledEventArgs args) {
            _tcs.TrySetException(args.Exception);
        }

        private static PipelineConfiguration CreatePipelineConfiguration(string filename, string outDir) {
            var stem = Path.GetFileNameWithoutExtension(filename);

            var config = new PipelineConfiguration() { 
                DeliveryPolicy = DeliveryPolicy.SynchronousOrThrottle,
            };
            var reader = new MediaSourceConfiguration() {
                Filename = filename,
            };
            config.Instances.Add(reader);
            var mediapipe = new MediaPipeConfiguration() {
                Inputs = { 
                    new InputConfiguration() {
                        LocalPort = new PortConfiguration() {
                            Identifier = "image",
                        },
                        RemoteId = reader.Id,
                        RemotePort = new PortConfiguration() {
                            Identifier = nameof(MediaSource.Image),
                        },
                    },
                },
            };
            config.Instances.Add(mediapipe);
            var libreface = new LibreFaceDetectorConfiguration() {
                DeliveryPolicy = DeliveryPolicy.Unlimited,
                Inputs = { 
                    new InputConfiguration() {
                        LocalPort = new PortConfiguration() {
                            Identifier = nameof(LibreFaceDetector.ImageIn),
                        },
                        RemoteId = reader.Id,
                        RemotePort = new PortConfiguration() {
                            Identifier = nameof(MediaSource.Image),
                        },
                    },
                    new InputConfiguration() {
                        LocalPort = new PortConfiguration() {
                            Identifier = nameof(LibreFaceDetector.DataIn),
                        },
                        RemoteId = mediapipe.Id,
                        RemotePort = new PortConfiguration() {
                            Identifier = "multi_face_landmarks",
                        },
                    },
                },
            };
            config.Instances.Add(libreface);
            var exporter = new JsonStoreExporterConfiguration() {
                StoreName = stem,
                RootPath = outDir,
                CreateSubdirectory = false,
                Inputs = {
                    new InputConfiguration() {
                        LocalPort = new PortConfiguration() {
                            Identifier = nameof(IConsumer<object>.In),
                            Index = nameof(LibreFaceDetector.ActionUnitPresenceOut),
                        },
                        RemoteId = libreface.Id,
                        RemotePort = new PortConfiguration() {
                            Identifier = nameof(LibreFaceDetector.ActionUnitPresenceOut),
                        },
                    },
                    new InputConfiguration() {
                        LocalPort = new PortConfiguration() {
                            Identifier = nameof(IConsumer<object>.In),
                            Index = nameof(LibreFaceDetector.ActionUnitIntensityOut),
                        },
                        RemoteId = libreface.Id,
                        RemotePort = new PortConfiguration() {
                            Identifier = nameof(LibreFaceDetector.ActionUnitIntensityOut),
                        },
                    },
                    new InputConfiguration() {
                        LocalPort = new PortConfiguration() {
                            Identifier = nameof(IConsumer<object>.In),
                            Index = nameof(LibreFaceDetector.FacialExpressionOut),
                        },
                        RemoteId = libreface.Id,
                        RemotePort = new PortConfiguration() {
                            Identifier = nameof(LibreFaceDetector.FacialExpressionOut),
                        },
                    },
                },
            };
            config.Instances.Add(exporter);
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
