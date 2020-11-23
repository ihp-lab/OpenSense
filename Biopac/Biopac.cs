using System;
using BiopacInterop;
using Microsoft.Psi;
using Microsoft.Psi.Components;

namespace OpenSense.Component.Biopac {
    /// <summary>
    /// StringProducer class.
    /// </summary>
    public class Biopac : Generator, IProducer<string> {

        private BiopacCommunicatorWrapper communicator;

        /// <summary>
        /// Initializes a new instance of the <see cref="StringProducer"/> class.
        /// </summary>
        /// <param name="pipeline">The pipeline.</param>
        public Biopac(Pipeline pipeline) : base(pipeline) {
            Out = pipeline.CreateEmitter<string>(this, nameof(Out));

            pipelineLocal = pipeline;

            communicator = new BiopacCommunicatorWrapper();
            communicator.StartCommunication();

            // Application exit callback
            pipeline.ComponentCompleted += OnExitMethod;
        }

        /// <summary>
        /// Gets. Emitter that encapsulates the string output stream.
        /// </summary>
        public Emitter<string> Out { get; }

        private void Reset() {
            if (communicator.getAcquisitionInProgress() == 1) {
                if (communicator.toggleAcquisition() == 0) {
                    Console.WriteLine("XML-RPC SERVER: toggleAcquisition() SUCCEEDED" + "\n" + "....." + "acquisition_progress = off");
                }
            }
        }

        /// <summary>
        /// Generates and time-stamps a string.
        /// </summary>
        protected override DateTime GenerateNext(DateTime previous) {
            string s = communicator.GetData().ToString();
            //string s = "Biopac";

            // No more data
            if (s == null) {
                return DateTime.MaxValue;
            }

            // Originating time.
            DateTime originatingTime = pipelineLocal.GetCurrentTime();

            Out.Post(s, originatingTime);

            return originatingTime;
        }

        /// <summary>
        /// Local pipeline.
        /// </summary>
        private readonly Pipeline pipelineLocal;

        /// <summary>
        /// Application exit method.
        /// </summary>
        private void OnExitMethod(object sender, EventArgs e) {
            Reset();
        }
    }
}
