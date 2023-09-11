#nullable enable

using Microsoft.Psi;

namespace OpenSense.Components.Builtin {
    public sealed class DataRateMeter : IConsumer<object?>, IProducer<double> {

        #region Ports
        public Receiver<object?> In { get; }

        public Emitter<double> Out { get; }
        #endregion

        public DataRateMeter(Pipeline pipeline) {
            
        }
    }
}
