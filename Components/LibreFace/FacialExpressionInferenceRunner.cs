using System;
using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.Psi;
using Microsoft.Psi.Components;
using Microsoft.Psi.Imaging;
using LibreFace;
using Microsoft.ML.OnnxRuntime;

namespace OpenSense.Components.LibreFace {
    internal sealed class FacialExpressionInferenceRunner : IConsumerProducer<IReadOnlyList<Shared<Image>>, IReadOnlyList<IReadOnlyDictionary<string, float>>>, IDisposable {

        private readonly FacialExpressionModelContext _modelContext;

        public Receiver<IReadOnlyList<Shared<Image>>> In { get; }

        public Emitter<IReadOnlyList<IReadOnlyDictionary<string, float>>> Out { get; }

        public FacialExpressionInferenceRunner(Pipeline pipeline) {
            In = pipeline.CreateReceiver<IReadOnlyList<Shared<Image>>>(this, Process, nameof(In));
            Out = pipeline.CreateEmitter<IReadOnlyList<IReadOnlyDictionary<string, float>>>(this, nameof(Out));
            var options =
#if CUDA
                SessionOptions.MakeSessionOptionWithCudaProvider(deviceId: 0);
#else
                new SessionOptions() {
                };
#endif
            _modelContext = new FacialExpressionModelContext(options, isOwner: true);
        }

        private void Process(IReadOnlyList<Shared<Image>> images, Envelope envelope) {
            if (disposed) {
                throw new ObjectDisposedException(nameof(FacialExpressionInferenceRunner));
            }

            var result = new List<ExpressionOutput>(images.Count);
            foreach (var image in images) {// Evaluate in serial
                Debug.Assert(image.Resource.PixelFormat == PixelFormat.RGB_24bpp);
                unsafe {
                    var span = new Span<byte>(image.Resource.ImageData.ToPointer(), image.Resource.Size);
                    using var input = new ImageInput(span, image.Resource.Stride);
                    var expression = _modelContext.Run(input);
                    result.Add(expression);
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
