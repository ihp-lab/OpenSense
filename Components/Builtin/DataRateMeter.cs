#nullable enable

using System;
using Microsoft.Psi;

namespace OpenSense.Components.Builtin {
    public sealed class DataRateMeter : IConsumer<object?>, IProducer<double> {
        private System.Timers.Timer? timer_ = null;
        private double prev_seconds = 0;

        #region Ports
        public Receiver<object?> In { get; }

        public Emitter<double> Out { get; }
        #endregion

        public DataRateMeter(Pipeline pipeline) {
            this.In = pipeline.CreateReceiver<object?>(this, Process, nameof(this.In));
            this.Out = pipeline.CreateEmitter<double>(this, nameof(this.Out));
        }

        public void createTimer() {
            this.timer_ = new System.Timers.Timer(100);

            this.timer_.Elapsed += checkStream;
            this.timer_.AutoReset = true;
            this.timer_.Enabled = true;
        }

        public void Process(object? n, Envelope envelope) {
            // Dispose timer_ if not null.
            if (this.timer_ != null) { this.timer_.Dispose(); }
            
            // Get Current Time.
            DateTime current_time = DateTime.Now;

            // Calculate Frame Rate
            double current_seconds = current_time.TimeOfDay.TotalSeconds, difference = 0;
            if (this.prev_seconds != 0) { 
                difference = 1.0d / (current_seconds - this.prev_seconds);
            }
            this.prev_seconds = current_seconds;
            this.Out.Post(difference, envelope.OriginatingTime);

            // Create New Timer
            createTimer();
        }

        public void checkStream(object? source, System.Timers.ElapsedEventArgs e) {
            DateTime current_time = DateTime.Now;

            double current_seconds = current_time.TimeOfDay.TotalSeconds, difference = 0;
            if (prev_seconds != 0) {
                difference = 1.0d / (current_seconds - this.prev_seconds);
            }

            // Post throws an error due to the fact that it says the "envelope" argument has a non-increasing OriginatingTime.
            try { this.Out.Post(difference, e.SignalTime); }
            catch (Exception err) {}
        }
    }
}
