using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MathNet.Spatial.Euclidean;
using OpenSense.DataStructure;

namespace OpenSense.GazeToDisplayConverter {
    [Serializable]
    public class TwoStageConverterParameters : GazeToDisplayConverterParameters {
        public int Order { get; set; }
        public Record[] Samples { get; set; }

        public TwoStageConverterParameters() {
            ConverterType = typeof(TwoStageConverter);
        }
    }
}
