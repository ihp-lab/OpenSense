using Microsoft.ML.OnnxRuntime;
using System.Diagnostics;
using System.Reflection;

namespace LibreFace {
    public sealed class ActionUnitIntensityModelContext : IDisposable {

        private const string InputName = "image";

        private const string OutputName = "AUs";

        private readonly string ModelFilename = Path.Combine(
            Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location),
            "LibreFace_AU_Intensity.onnx"
            );

        private readonly SessionOptions _options;

        private readonly InferenceSession _session;

        public ActionUnitIntensityModelContext() {
            Debug.Assert(File.Exists(ModelFilename));

#if CUDA
            _options = SessionOptions.MakeSessionOptionWithCudaProvider(deviceId: 0);
#else
            _options = new SessionOptions() {
            };
#endif

            _session = new InferenceSession(ModelFilename, _options);
        }

        public ActionUnitIntensityOutput Run(ImageInput image) {
            if (disposed) {
                throw new ObjectDisposedException(nameof(ActionUnitIntensityModelContext));
            }
            var inputs = new NamedOnnxValue[1];
            inputs[0] = NamedOnnxValue.CreateFromTensor(InputName, image.Tensor);
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
            _options.Dispose();

            disposed = true;
        }
        #endregion
    }
}
