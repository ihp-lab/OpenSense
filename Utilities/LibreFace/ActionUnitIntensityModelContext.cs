using Microsoft.ML.OnnxRuntime;
using System.Diagnostics;
using System.Reflection;

namespace LibreFace {

    public sealed class ActionUnitIntensityModelContext : IDisposable {

        private const string InputName = "feature";

        private const string OutputName = "au_intensity";

        private readonly string ModelFilename = Path.Combine(
            Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location),
            "LibreFace_AU_Intensity.onnx"
            );

        private readonly SessionOptions? _options;

        private readonly InferenceSession _session;

        public ActionUnitIntensityModelContext() : this(new SessionOptions(), true) { 
        }

        public ActionUnitIntensityModelContext(SessionOptions options, bool isOwner) {
            Debug.Assert(File.Exists(ModelFilename));
            _session = new InferenceSession(ModelFilename, options);
            if (isOwner) {
                _options = options;
            }
        }

        public ActionUnitIntensityOutput Run(ActionUnitFeatureInput feature) {
            if (disposed) {
                throw new ObjectDisposedException(nameof(ActionUnitIntensityModelContext));
            }
            var inputs = new NamedOnnxValue[1];
            inputs[0] = NamedOnnxValue.CreateFromTensor(InputName, feature.Tensor);
            using var outputs = _session.Run(inputs);
            var output = outputs.Single(o => o.Name == OutputName);
            var result = new ActionUnitIntensityOutput(output);
            return result;
        }

        #region IDisposable
        private bool disposed;

        public void Dispose() {
            if (disposed) {
                return;
            }

            _session.Dispose();
            _options?.Dispose();

            disposed = true;
        }
        #endregion
    }
}
