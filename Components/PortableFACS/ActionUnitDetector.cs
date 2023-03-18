using System;
using System.Collections.Generic;
using Microsoft.Psi;
using Microsoft.Psi.Components;
using Microsoft.Psi.Imaging;

namespace OpenSense.Components.PortableFACS {
    public sealed class ActionUnitDetector : IConsumerProducer<Shared<Image>, IReadOnlyDictionary<int, float>> {
        public Receiver<Shared<Image>> In { get; }

        public Emitter<IReadOnlyDictionary<int, float>> Out { get; }

        public ActionUnitDetector(Pipeline pipeline) {
            In = pipeline.CreateReceiver<Shared<Image>>(this, Process, nameof(In));
            Out = pipeline.CreateEmitter<IReadOnlyDictionary<int, float>>(this, nameof(Out));
        }

        private void Process(Shared<Image> image, Envelope envelope) {
            if (image is null) {
                return;
            }

            throw new NotImplementedException();
        }
    }
}
