using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.ML;
using Microsoft.Psi;
using Microsoft.Psi.Components;
using OpenSense.Component.Head.Common;

namespace OpenSense.Component.HeadGesture {
    internal class HeadposeRNN : IConsumerProducer<Pose, float> {
        private string modelFilePath;
        private OnnxRNNScorer modelScorer;
        private MLContext mlContext;
        private string assetsPath;
        private LinkedList<List<float>> dataQueueTransDiff = new LinkedList<List<float>>();
        private LinkedList<List<float>> dataQueueTTRRDiffDiff2 = new LinkedList<List<float>>();
        private List<float> lastInputTRTRDiffDiff2 = new List<float>();
        private const int NUM_OF_FRAME = 32;
        private const int NUM_OF_FEATURES = 12;

        public HeadposeRNN(Pipeline pipeline, string modelNameFile, ModelSettings modelSetting) {

            // Create the receiver.
            In = pipeline.CreateReceiver<Pose>(this, ReceiveData, nameof(In));

            // Create the emitter.
            Out = pipeline.CreateEmitter<float>(this, nameof(Out));

            // location of the model relative to .exe
            modelFilePath = OnnxUtility.ModelAbsoluteFilename(modelNameFile);

            // create a scorer which has the model and setting
            mlContext = new MLContext();
            modelScorer = new OnnxRNNScorer(modelFilePath, mlContext, modelSetting);
            List<float> zerosList = new List<float>(new float[NUM_OF_FEATURES]);
            for (int i = 0; i < NUM_OF_FRAME; i++) {
                dataQueueTransDiff.AddLast(zerosList);
                dataQueueTTRRDiffDiff2.AddLast(zerosList);
            }
            lastInputTRTRDiffDiff2 = zerosList;
        }
        /// <summary>
        /// Receiver that encapsulates the data input stream.
        /// </summary>
        public Receiver<Pose> In {
            get;
            private set;
        }

        /// <summary>
        /// Emitter that encapsulates the data output stream.
        /// </summary>
        public Emitter<float> Out {
            get;
            private set;
        }

        private List<float> deepCopyList(List<float> input) {
            List<float> output = new List<float>();
            for (int i = 0; i < input.Count(); i++) {
                output.Add(input[i]);
            }
            return output;
        }
        private void ReceiveData(Pose input, Envelope envelope) {
            // input: T_x, T_y, T_z, R_x, R_y, R_z
            List<float> inputTRTRTransDiff = new List<float>(); //T_x, T_y, T_z, R_x, R_y, R_z, diff_Tx, diff_Ty, diff_Tz, diff_Rx, diff_Ry, diff_Rz
            List<float> inputTRTRDiffDiff2 = new List<float>(); //diff_Tx, diff_Ty, diff_Tz, diff_Rx, diff_Ry, diff_Rz, diff2_Tx, diff2_Ty, diff2_Tz, diff2_Rx, diff2_Ry, diff2_Rz
            // calculate diff of T_x, T_y, T_z, R_x, R_y, R_z

            for (int i = 0; i < input.Count(); i++) {
                inputTRTRTransDiff.Add((float)input[i]);
            }
            for (int i = 0; i < input.Count(); i++) {
                inputTRTRTransDiff.Add((float)input[i] - dataQueueTransDiff.Last()[i]);
            }
            // calculate diff and diff2 of T_x, T_y, T_z, R_x, R_y, R_z
            // add diff

            for (int i = 0; i < input.Count(); i++) {
                inputTRTRDiffDiff2.Add(inputTRTRTransDiff[input.Count + i]);
            }
            // add diff2
            for (int i = 0; i < input.Count(); i++) {
                inputTRTRDiffDiff2.Add(inputTRTRDiffDiff2[i] - lastInputTRTRDiffDiff2[i]);
            }


            // reorder into diff_Tx, diff_Ty, diff2_Tx, diff2_Ty, diff2_Tz, diff_Tz, diff_Rx, diff_Ry, diff_Rz, diff2_Rx, diff2_Ry, diff2_Rz
            List<float> inputTTRRDiffDiff2 = deepCopyList(inputTRTRDiffDiff2);


            for (int i = 3; i < 6; i++) {
                inputTTRRDiffDiff2[i] = inputTRTRDiffDiff2[i + 3];
            }
            for (int i = 6; i < 9; i++) {
                inputTTRRDiffDiff2[i] = inputTRTRDiffDiff2[i - 3];
            }


            lastInputTRTRDiffDiff2 = inputTRTRDiffDiff2;
            dataQueueTransDiff.AddLast(inputTRTRTransDiff);
            dataQueueTTRRDiffDiff2.AddLast(inputTTRRDiffDiff2);


            if (dataQueueTransDiff.Count() > NUM_OF_FRAME) {
                dataQueueTransDiff.RemoveFirst();
            }
            if (dataQueueTTRRDiffDiff2.Count() > NUM_OF_FRAME) {
                dataQueueTTRRDiffDiff2.RemoveFirst();
            }

            // flatten the whole list
            List<float> networkInput = new List<float>();
            List<List<float>> dataQueueList = dataQueueTTRRDiffDiff2.ToList();


            //row major
            for (int i = 0; i < dataQueueList.Count(); i++) {
                for (int j = 0; j < dataQueueList[i].Count; j++) {
                    networkInput.Add(dataQueueList[i][j]);
                }
            }

            try {
                // evaluate using the model
                HeadPoseObject headPoseObj = new HeadPoseObject(networkInput);
                IEnumerable<HeadPoseObject> images = headPoseObj.getHeadPoseIEnumerable();
                IDataView imageDataView = mlContext.Data.LoadFromEnumerable(images);
                IEnumerable<float[]> probabilities = modelScorer.Score(imageDataView);
                var probList = probabilities.ToList();

                Out.Post(probList[0][31], envelope.OriginatingTime);
            } catch (Exception ex) {
                Console.WriteLine(ex.ToString());
            }
        }

    }
}
