using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenSense.Components.Onnx {
    // image setting for input image. Input will be resize according to the setting
    public struct ImageNetSettings {
        public int imageHeight;
        public int imageWidth;
    }
}
