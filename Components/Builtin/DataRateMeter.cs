#nullable enable

using System;
using Microsoft.Psi;

namespace OpenSense.Components.Builtin {
    public sealed class DataRateMeter : IConsumer<object?>, IProducer<double> {
        private System.Threading.Timer? timer_ = null;
        private double prev_seconds = 0;

        #region Ports
        public Receiver<object?> In { get; }

        public Emitter<double> Out { get; }
        #endregion

        public DataRateMeter(Pipeline pipeline) {
            this.In = pipeline.CreateReceiver<object?>(this, Process, nameof(this.In));
            this.Out = pipeline.CreateEmitter<double>(this, nameof(this.Out));
            timer_ = new System.Threading.Timer(checkStream, null, 1000, 1000);
        }

        public void Process(object? n, Envelope envelope) {
            DateTime current_time = DateTime.Now;

            double current_seconds = current_time.TimeOfDay.TotalSeconds, difference = 0;
            if (prev_seconds != 0) { 
                difference = 1.0d / (current_seconds - this.prev_seconds);
            }
            this.prev_seconds = current_seconds;
            this.Out.Post(difference, envelope.OriginatingTime);
        }

        public void checkStream(object? state) { 
        }
    }
}
