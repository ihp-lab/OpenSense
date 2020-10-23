using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenSense.Components.Onnx {
    // normalization parameter for each channel
    public struct NormalizationSettings {
        public float redStd;
        public float greenStd;
        public float blueStd;
        public float redMean;
        public float greenMean;
        public float blueMean;
    }
}
