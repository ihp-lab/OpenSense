using System;
using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.Psi;
using Microsoft.Psi.Components;
using Microsoft.Psi.Imaging;
using LibreFace;

namespace OpenSense.Components.LibreFace {
    internal sealed class ActionUnitPresenceInferenceRunner : IConsumerProducer<IReadOnlyList<Shared<Image>>, IReadOnlyList<IReadOnlyDictionary<string, bool>>>, IDisposable {

        private readonly ActionUnitPresenceModelContext _modelContext;

        public Receiver<IReadOnlyList<Shared<Image>>> In { get; }

        public Emitter<IReadOnlyList<IReadOnlyDictionary<string, bool>>> Out { get; }

        public ActionUnitPresenceInferenceRunner(Pipeline pipeline) {
            In = pipeline.CreateReceiver<IReadOnlyList<Shared<Image>>>(this, Process, nameof(In));
            Out = pipeline.CreateEmitter<IReadOnlyList<IReadOnlyDictionary<string, bool>>>(this, nameof(Out));
            _modelContext = new ActionUnitPresenceModelContext();
        }

        private void Process(IReadOnlyList<Shared<Image>> images, Envelope envelope) {
            if (disposed) {
                throw new ObjectDisposedException(nameof(ActionUnitPresenceInferenceRunner));
            }

            var result = new List<ActionUnitPresenceOutput>(images.Count);
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
