using System.IO;
using System.Reflection;


namespace OpenSense.Component.HeadGesture {
    internal static class OnnxUtility {

        private const string MODEL_PATH = "Models";

        public static string ModelAbsoluteFilename(string modelFilename) {
            var asm = Assembly.GetExecutingAssembly().Location;
            var dir = Path.GetDirectoryName(asm);
            var filename = Path.Combine(dir, MODEL_PATH, modelFilename);
            return filename;
        }
    }
}
