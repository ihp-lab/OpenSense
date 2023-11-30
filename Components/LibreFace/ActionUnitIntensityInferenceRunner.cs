using System;
using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.Psi;
using Microsoft.Psi.Components;
using Microsoft.Psi.Imaging;
using LibreFace;
using Microsoft.ML.OnnxRuntime;

namespace OpenSense.Components.LibreFace {
    internal sealed class ActionUnitIntensityInferenceRunner : IConsumerProducer<IReadOnlyList<Shared<Image>>, IReadOnlyList<IReadOnlyDictionary<string, float>>>, IDisposable {

        private readonly ActionUnitIntensityModelContext _modelContext;

        public Receiver<IReadOnlyList<Shared<Image>>> In { get; }

        public Emitter<IReadOnlyList<IReadOnlyDictionary<string, float>>> Out { get; }

        public ActionUnitIntensityInferenceRunner(Pipeline pipeline) {
            In = pipeline.CreateReceiver<IReadOnlyList<Shared<Image>>>(this, Process, nameof(In));
            Out = pipeline.CreateEmitter<IReadOnlyList<IReadOnlyDictionary<string, float>>>(this, nameof(Out));
            var options =
#if CUDA
                SessionOptions.MakeSessionOptionWithCudaProvider(deviceId: 0);
#else
                new SessionOptions() {
                };
#endif
            _modelContext = new ActionUnitIntensityModelContext(options, isOwner: true);
        }

        private void Process(IReadOnlyList<Shared<Image>> images, Envelope envelope) {
            if (disposed) {
                throw new ObjectDisposedException(nameof(ActionUnitIntensityInferenceRunner));
            }

            var result = new List<ActionUnitIntensityOutput>(images.Count);
            foreach (var image in images) {// Evaluate in serial
                Debug.Assert(image.Resource.PixelFormat == PixelFormat.RGB_24bpp);
                unsafe {
                    var span = new Span<byte>(image.Resource.ImageData.ToPointer(), image.Resource.Size);
                    using var input = new ImageInput(span, image.Resource.Stride);
                    var au = _modelContext.Run(input);
                    result.Add(au);
                }
            }
            Out.Post(result, envelope.OriginatingTime);
        }

        #region IDisposable
        private bool disposed;

        public void Dispose() {
            if (disposed) {
                return;
            }

            _modelContext.Dispose();

            disposed = true;
        }
        #endregion
    }
}
