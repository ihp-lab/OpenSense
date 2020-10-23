using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;


namespace OpenSense.Components.Onnx {
    internal static class OnnxUtility {

        private const string MODEL_PATH = "Resource/Models/ONNX";

        public static string ModelAbsoluteFilename(string modelFilename) {
            var asm = Assembly.GetExecutingAssembly().Location;
            var dir = Path.GetDirectoryName(asm);
            var filename = Path.Combine(dir, MODEL_PATH, modelFilename);
            return filename;
        }
    }
}
