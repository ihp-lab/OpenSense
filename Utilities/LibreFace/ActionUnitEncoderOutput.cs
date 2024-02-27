using System.Collections;
using Microsoft.ML.OnnxRuntime;

namespace LibreFace {
    public sealed record class ActionUnitEncoderOutput : IReadOnlyList<float> {

        internal readonly float[] _buffer;

        internal ActionUnitEncoderOutput(in NamedOnnxValue feature) {
            _buffer = feature.AsEnumerable<float>().ToArray();
        }

        #region IReadOnlyList
        public float this[int index] => _buffer[index];

        public int Count => _buffer.Length;

        public IEnumerator<float> GetEnumerator() => ((IReadOnlyList<float>)_buffer).GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => _buffer.GetEnumerator();
        #endregion
    }
}
