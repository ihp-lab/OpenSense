using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenSense.GazeToDisplayConverter {
    public class EndToEndConverterParameters : GazeToDisplayConverterParameters {

        public byte[] PredictorX { get; set; }
        public byte[] PredictorY { get; set; }

        public EndToEndConverterParameters() {
            ConverterType = typeof(EndToEndConverter);
        }
    }
}
