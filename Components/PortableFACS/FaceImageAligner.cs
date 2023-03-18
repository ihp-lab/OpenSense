using System.Collections.Generic;
using Mediapipe.Net.Framework.Protobuf;
using Microsoft.Psi;
using Microsoft.Psi.Imaging;

namespace OpenSense.Components.PortableFACS {
    public sealed class FaceImageAligner : 
        IConsumer<(Shared<Image>, IReadOnlyList<NormalizedLandmarkList>)>, 
        IProducer<IReadOnlyList<Shared<Image>>> 
        {

        public Receiver<(Shared<Image>, IReadOnlyList<NormalizedLandmarkList>)> In { get; }

        public Emitter<IReadOnlyList<Shared<Image>>> Out { get; }

        public FaceImageAligner(Pipeline pipeline) {
            In = pipeline.CreateReceiver<(Shared<Image>, IReadOnlyList<NormalizedLandmarkList>)>(this, Process, nameof(In));
            Out = pipeline.CreateEmitter<IReadOnlyList<Shared<Image>>>(this, nameof(Out));
        }

        private void Process((Shared<Image>, IReadOnlyList<NormalizedLandmarkList>) data, Envelope envelope) {
            var (image, lmList) = data;
            foreach (var lm in lmList) {

            }
        }
    }
}
