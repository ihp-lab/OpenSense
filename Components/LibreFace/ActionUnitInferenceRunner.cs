using System;
using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.Psi;
using Microsoft.Psi.Imaging;
using LibreFace;
using Microsoft.ML.OnnxRuntime;

namespace OpenSense.Components.LibreFace {
    internal sealed class ActionUnitInferenceRunner : IConsumer<IReadOnlyList<Shared<Image>>>, IDisposable {

        private readonly ActionUnitEncoderModelContext _encoderContext;

        private readonly ActionUnitIntensityModelContext? _intensityContext;

        private readonly ActionUnitPresenceModelContext? _presenceContext;

        public Receiver<IReadOnlyList<Shared<Image>>> In { get; }

        public Emitter<IReadOnlyList<IReadOnlyDictionary<string, float>>> IntensityOut { get; }

        public Emitter<IReadOnlyList<IReadOnlyDictionary<string, bool>>> PresenceOut { get; }

        public ActionUnitInferenceRunner(Pipeline pipeline, bool intensity, bool presense) {
            In = pipeline.CreateReceiver<IReadOnlyList<Shared<Image>>>(this, Process, nameof(In));
            IntensityOut = pipeline.CreateEmitter<IReadOnlyList<IReadOnlyDictionary<string, float>>>(this, nameof(IntensityOut));
            PresenceOut = pipeline.CreateEmitter<IReadOnlyList<IReadOnlyDictionary<string, bool>>>(this, nameof(PresenceOut));
            var options =
#if CUDA
            SessionOptions.MakeSessionOptionWithCudaProvider(deviceId: 0);
#else
            new SessionOptions() {
            };
#endif
            _encoderContext = new ActionUnitEncoderModelContext(options, isOwner: true);
            if (intensity) {
                _intensityContext = new ActionUnitIntensityModelContext(options, isOwner: true);
            }
            if (presense) {
                _presenceContext = new ActionUnitPresenceModelContext(options, isOwner: true);
            }
        }

        private void Process(IReadOnlyList<Shared<Image>> images, Envelope envelope) {
            if (disposed) {
                throw new ObjectDisposedException(nameof(ActionUnitInferenceRunner));
            }

            var intensityResults = _intensityContext is null ? null : new List<ActionUnitIntensityOutput>(images.Count);
            var presenseResults = _presenceContext is null ? null : new List<ActionUnitPresenceOutput>(images.Count);
            foreach (var image in images) {// Evaluate in serial
                Debug.Assert(image.Resource.PixelFormat == PixelFormat.RGB_24bpp);
                unsafe {
                    var span = new Span<byte>(image.Resource.ImageData.ToPointer(), image.Resource.Size);
                    using var input = new ImageInput(span, image.Resource.Stride);
                    var feature = _encoderContext.Run(input);
                    if (_intensityContext is not null) {
                        var intensity = _intensityContext.Run(feature);
                        intensityResults!.Add(intensity);
                    }
                    if (_presenceContext is not null) {
                        var presence = _presenceContext.Run(feature);
                        presenseResults!.Add(presence);
                    }
                }
            }

            if (intensityResults is not null) {
                IntensityOut.Post(intensityResults, envelope.OriginatingTime);
            }
            if (presenseResults is not null) {
                PresenceOut.Post(presenseResults, envelope.OriginatingTime);
            }
        }

        #region IDisposable
        private bool disposed;

        public void Dispose() {
            if (disposed) {
                return;
            }

            _encoderContext.Dispose();
            _intensityContext?.Dispose();
            _presenceContext?.Dispose();

            disposed = true;
        }
        #endregion
    }
}
