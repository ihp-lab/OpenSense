using Microsoft.ML.OnnxRuntime.Tensors;
using System.Buffers;
using System.Diagnostics;

namespace PortableFACS {
    public sealed class ImageInput : IDisposable {

        private const int ChannelSize = 3;

        public const int InputSize = 224;//256;

        private const int CropSize = 224;

        private const int BorderSize = (InputSize - CropSize) / 2;

        private const int StopIdx = BorderSize + CropSize;

        private const int BufferLength = ChannelSize * CropSize * CropSize;//C * H * W

        private static readonly float[] Mean = {
            0.485f,
            0.456f,
            0.406f,
        };

        private static readonly float[] Std = {
            0.229f,
            0.224f,
            0.225f,
        };

        private static readonly int[] Dims = { 
            1,
            ChannelSize,
            CropSize,
            CropSize,
        };

        private readonly float[] _buffer;

        private readonly Lazy<DenseTensor<float>> _tensor;

        internal DenseTensor<float> Tensor => _tensor.Value;

        public ImageInput(ReadOnlySpan<byte> buffer, int stride) {
            Debug.Assert(stride >= InputSize);
            Debug.Assert(buffer.Length == InputSize * stride);//H * (W * C)

            _buffer = ArrayPool<float>.Shared.Rent(BufferLength);

            //TODO: Vectorize
            for (var i = BorderSize; i < StopIdx; i++) {//row
                for (var j = BorderSize; j < StopIdx; j++) {//col
                    for (var k = 0; k < ChannelSize; k++) {//ch
                        var srcIdx = i * stride + j * ChannelSize + k;
                        var dstIdx = (k * InputSize + i) * InputSize + j;
                        _buffer[dstIdx] = (Math.Clamp(1f / byte.MaxValue * Convert.ToSingle(buffer[srcIdx]), 0, 1) - Mean[k]) / Std[k];
                    }
                }
            }

            _tensor = new Lazy<DenseTensor<float>>(() => new DenseTensor<float>(_buffer[..BufferLength].AsMemory(), Dims), LazyThreadSafetyMode.PublicationOnly);
        }

        #region IDisposable
        private bool _disposed;

        public void Dispose() {
            if (_disposed) {
                return;
            }
            ArrayPool<float>.Shared.Return(_buffer);
            _disposed = true;
        }
        #endregion
    }
}
