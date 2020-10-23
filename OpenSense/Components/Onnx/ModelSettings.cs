using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenSense.Components.Onnx {
    /// <summary>
    /// for checking Model input and  output  parameter names,
    ///you can use tools like Netron, 
    /// </summary>
    public struct ModelSettings {
        public string modelInput;
        public string modelOutput;
    }
}
