﻿using Microsoft.ML.OnnxRuntime;
using System.Diagnostics;
using System.Reflection;

namespace LibreFace {
    public sealed class FacialExpressionModelContext : IDisposable {

        private const string InputName = "image";

        private const string OutputName = "FEs";

        private readonly string ModelFilename = Path.Combine(
            Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location),
            "LibreFace_FE.onnx"
            );

        private readonly SessionOptions? _options;

        private readonly InferenceSession _session;

        public FacialExpressionModelContext() : this(new SessionOptions(), true) {
        }

        public FacialExpressionModelContext(SessionOptions options, bool isOwner) {
            Debug.Assert(File.Exists(ModelFilename));
            _session = new InferenceSession(ModelFilename, options);
            if (isOwner) {
                _options = options;
            }
        }

        public ExpressionOutput Run(ImageInput image) {
            if (disposed) {
                throw new ObjectDisposedException(nameof(FacialExpressionModelContext));
            }
            var inputs = new NamedOnnxValue[1];
            inputs[0] = NamedOnnxValue.CreateFromTensor(InputName, image.Tensor);
            using var outputs = _session.Run(inputs);
            var output = outputs.Single(o => o.Name == OutputName);
            var result = new ExpressionOutput(output);
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
