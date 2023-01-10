using System.Collections.Generic;
using System.Linq;
using Microsoft.Psi;
using HeadGestureData = OpenSense.Components.OpenFace.Gesture;

namespace OpenSense.Components.HeadGesture {
    /// <summary>
    /// comparing shake nod and tilt, the category of highest value get stream out
    /// </summary>
    public class ShakeNodTiltCompare: IProducer<HeadGestureData> {
        private float shakeProb = 0f;
        private float nodProb = 0f;
        private float tiltProb = 0f;

        public ShakeNodTiltCompare(Pipeline pipeline) {

            // Create the receiver.
            ShakeIn = pipeline.CreateReceiver<float>(this, ReceiveShake, nameof(ShakeIn));
            NodIn = pipeline.CreateReceiver<float>(this, UpdateNod, nameof(NodIn));
            TiltIn = pipeline.CreateReceiver<float>(this, UpdateTilt, nameof(TiltIn));


            // Create the emitter.
            Out = pipeline.CreateEmitter<HeadGestureData>(this, nameof(Out));


        }
        /// <summary>
        /// Receiver that encapsulates the data input stream.
        /// </summary>
        public Receiver<float> ShakeIn {
            get;
            private set;
        }

        /// <summary>
        /// Receiver that encapsulates the data input stream.
        /// </summary>
        public Receiver<float> NodIn {
            get;
            private set;
        }

        /// <summary>
        /// Receiver that encapsulates the data input stream.
        /// </summary>
        public Receiver<float> TiltIn {
            get;
            private set;
        }

        /// <summary>
        /// Emitter that encapsulates the data output stream.
        /// </summary>
        public Emitter<HeadGestureData> Out {
            get;
            private set;
        }
        private void ReceiveShake(float input, Envelope envelope) {
            shakeProb = input;
            List<float> prob = new List<float>();
            prob.Add(shakeProb);
            prob.Add(nodProb);
            prob.Add(tiltProb);
            float max = shakeProb;
            int maxIndex = 0;
            for (int i = 0; i < prob.Count(); i++) {
                if (prob[i] > max) {
                    max = prob[i];
                    maxIndex = i;
                }
            }
            var result = HeadGestureData.None;
            if (max > 0.9) {
                switch (maxIndex) {
                    case 0:
                        result = HeadGestureData.Shake;
                        break;
                    case 1:
                        result = HeadGestureData.Nod;
                        break;
                    case 2:
                        result = HeadGestureData.Tilt;
                        break;
                }
            }
            Out.Post(result, envelope.OriginatingTime);
        }

        private void UpdateNod(float input, Envelope envelope) {
            nodProb = input;
        }
        private void UpdateTilt(float input, Envelope envelope) {
            tiltProb = input;
        }

    }


}
