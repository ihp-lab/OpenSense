#nullable enable

using Microsoft.Psi;

namespace OpenSense.Components.Builtin {
    public sealed class ToString : IConsumer<object?>, IProducer<string?> {

        #region Ports
        public Receiver<object?> In { get; }

        public Emitter<string?> Out { get; }
        #endregion

        public ToString(Pipeline pipeline) {
            In = pipeline.CreateReceiver<object?>(this, Process, nameof(In));
            Out = pipeline.CreateEmitter<string?>(this, nameof(Out));
        }

        private void Process(object? data, Envelope envelope) {
            var result = data?.ToString();
            Out.Post(result, envelope.OriginatingTime);
        }
    }
}
