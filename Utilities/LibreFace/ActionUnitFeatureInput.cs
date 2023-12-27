using System.Collections;
using Microsoft.ML.OnnxRuntime.Tensors;

namespace LibreFace {
    public sealed record class ActionUnitFeatureInput : IReadOnlyList<float> {

        private static readonly int[] Dims = {
            1,
            512,
        };

        private readonly float[] _buffer;

        private readonly Lazy<DenseTensor<float>> _tensor;

        internal DenseTensor<float> Tensor => _tensor.Value;

        internal ActionUnitFeatureInput(in ActionUnitEncoderOutput feature) {
            _buffer = feature._buffer;

            DenseTensor<float> createTensor() => new DenseTensor<float>(_buffer.AsMemory(), Dims);
            _tensor = new Lazy<DenseTensor<float>>(createTensor, LazyThreadSafetyMode.PublicationOnly);
        }

        #region IReadOnlyList
        public float this[int index] => _buffer[index];

        public int Count => _buffer.Length;

        public IEnumerator<float> GetEnumerator() => ((IReadOnlyList<float>)_buffer).GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => _buffer.GetEnumerator();
        #endregion

        #region Converters
        public static implicit operator ActionUnitFeatureInput(ActionUnitEncoderOutput feature) => new ActionUnitFeatureInput(feature);
        #endregion
    }
}
