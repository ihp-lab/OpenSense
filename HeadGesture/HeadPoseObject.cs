using System.Collections.Generic;
using System.Linq;
using Microsoft.ML.Data;

namespace OpenSense.Component.HeadGesture {
    // the object for input of the neural nework. Pass into the network in each inference. The name of the member variable should match the first input column when loading model pipeline
    internal class HeadPoseObject {
        private const int LIST_SIZE = 384;
        // specifying the size of input and name of input for the model. Using Netron to check the name
        [ColumnName("masking_1_input"), VectorType(32, 12)]
        public float[] headPose { get; set; }

        /// <summary>
        /// create empty input
        /// </summary>
        public HeadPoseObject() {
            headPose = new float[LIST_SIZE];
        }
        /// <summary>
        /// create input with 1-D list, which takes in LIST_SIZE
        /// </summary>
        /// <param name="input"></param>
        public HeadPoseObject(List<float> input) {
            headPose = new float[LIST_SIZE];
            if (input.Count == LIST_SIZE) {
                for (int i = 0; i < input.Count(); i++) {
                    headPose[i] = input[i];
                }
            } else {
                throw new System.ArgumentException("List size needs to be:" + LIST_SIZE.ToString() + "currently is: " + input.Count().ToString(), "input");
            }
        }
        /// <summary>
        /// ML.NET uses IEnumerable to pass into the input layer
        /// </summary>
        /// <returns>IEnumerable of this object</returns>
        public IEnumerable<HeadPoseObject> getHeadPoseIEnumerable() {
            IEnumerable<HeadPoseObject> objects = new HeadPoseObject[] { this };
            return objects;
        }
    }
}
