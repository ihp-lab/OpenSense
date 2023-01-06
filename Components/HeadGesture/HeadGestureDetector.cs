using Microsoft.Psi;
using Microsoft.Psi.Components;
using OpenSense.Components.OpenFace.Common;
using HeadGestureData = OpenSense.Components.OpenFace.Common.Gesture;

namespace OpenSense.Components.HeadGesture {
    public class HeadGestureDetector : Subpipeline, IConsumerProducer<Pose, HeadGestureData> {
        private const string INPUT_NAME = "masking_1_input";
        private const string OUTPUT_NAME = "time_distributed_1";
        private const string SHAKE_MODEL_NAME = "final_4comb_shake_32ws_12f_8u.onnx";
        private const string NOD_MODEL_NAME = "final_4comb_nod_32ws_12f_16u.onnx";
        private const string TILT_MODEL_NAME = "final_4comb_tilt_32ws_12f_16u.onnx";

        // Connector for the string input
        private Connector<Pose> dataIn;

        // Constructor
        public HeadGestureDetector(Pipeline pipeline) : base(pipeline, nameof(HeadGestureDetector)) {
            // Create the connectors
            dataIn = CreateInputConnectorFrom<Pose>(pipeline, nameof(In));

            // Define the outputs
            var enumOut = CreateOutputConnectorTo<HeadGestureData>(pipeline, nameof(Out));

            Out = enumOut.Out;

            // Create the string headpose rnn component, and connect it
            HeadposeRNN shakeComponent = new HeadposeRNN(this, SHAKE_MODEL_NAME, new ModelSettings { modelInput = INPUT_NAME, modelOutput = OUTPUT_NAME });
            HeadposeRNN nodComponent = new HeadposeRNN(this, NOD_MODEL_NAME, new ModelSettings { modelInput = INPUT_NAME, modelOutput = OUTPUT_NAME });
            HeadposeRNN tiltComponent = new HeadposeRNN(this, TILT_MODEL_NAME, new ModelSettings { modelInput = INPUT_NAME, modelOutput = OUTPUT_NAME });
            ShakeNodTiltCompare shakeNodTiltCompare = new ShakeNodTiltCompare(this);
            dataIn.Out.PipeTo(shakeComponent.In);
            dataIn.Out.PipeTo(nodComponent.In);
            dataIn.Out.PipeTo(tiltComponent.In);
            shakeComponent.Out.PipeTo(shakeNodTiltCompare.ShakeIn);
            nodComponent.Out.PipeTo(shakeNodTiltCompare.NodIn);
            tiltComponent.Out.PipeTo(shakeNodTiltCompare.TiltIn);
            shakeNodTiltCompare.Out.PipeTo(enumOut);

        }

        // Receiver for string input
        public Receiver<Pose> In => dataIn.In;

        // Emitter for string output
        public Emitter<HeadGestureData> Out { get; private set; }

    }
}

