using System.Buffers;
using System.Collections.Immutable;
using System.Diagnostics;

namespace NumPy {
    public abstract class ndarray {
    }

    public abstract class ndarray<T>:
        ndarray
        where T :
            struct
    {

        public ImmutableArray<int> shape { get; private protected init; }

        internal bool IsReadOnly { get; init; }

        public abstract ndarray<T> this[params object[] indices] { get; set; }

        public abstract T this[params int[] indices] { get; set; }

        protected int GetIndex(IReadOnlyList<int> indices) {
            Debug.Assert(indices.Count == shape.Length);
            var result = 0;
            var stride = 1;
            for (var i = shape.Length - 1; i >= 0; i--) {
                result += indices[i] * stride;
                stride *= shape[i];
            }
            return result;
        }
    }

    internal sealed class ndarray_Storage<T>:
        ndarray<T>
        where T : 
            struct
    {

        private readonly IMemoryOwner<T> _owner;

        public override ndarray<T> this[params object[] indices] {
            get => new ndarray_Slice<T>(this, indices);
            set => new ndarray_Slice<T>(this, indices).Assign(value);
        }

        public override T this[params int[] indices] {
            get => _owner.Memory.Span[GetIndex(indices)];
            set {
                Debug.Assert(!IsReadOnly);
                _owner.Memory.Span[GetIndex(indices)] = value;
            }
        }

        public ndarray_Storage(T[] values) {
            _owner = MemoryPool<T>.Shared.Rent(values.Length);
            values.CopyTo(_owner.Memory);
            shape = ImmutableArray.Create(values.Length);
        }

        ~ndarray_Storage() { 
            _owner.Dispose();
        }
    }

    internal sealed class ndarray_Slice<T>:
        ndarray<T>
        where T :
            struct
    {

        private readonly ndarray<T> _parent;

        private readonly ImmutableArray<object> _ranges;

        public override ndarray<T> this[params object[] indices] {
            get => throw new NotImplementedException();
            set => throw new NotImplementedException();
        }

        public override T this[params int[] indices] {
            get => throw new NotImplementedException();
            set => throw new NotImplementedException();
        }

        public ndarray_Slice(ndarray<T> parent, object[] indices) {
            Debug.Assert(TestIndices(parent, indices));
            _ranges = indices.ToImmutableArray();
            _parent = parent;
            shape = GetShape(parent, indices);
        }

        public void Assign(ndarray<T> value) {
            Debug.Assert(value.shape.SequenceEqual(_parent.shape));

        }

        #region Helpers
        private static bool TestIndices(ndarray<T> array, object[] indices) {
            if (array.shape.Length < indices.Length) {
                return false;
            }
            for (var i = 0; i < indices.Length; i++) {
                switch (indices[i]) {
                    case Index index:
                        var offset = index.GetOffset(array.shape[i]);
                        if (offset < 0 || offset >= array.shape[i]) {
                            return false;
                        }
                        break;
                    case Range range:
                        var (start, length) = range.GetOffsetAndLength(array.shape[i]);
                        if (start < 0 || start + length > array.shape[i]) {
                            return false;
                        }
                        break;
                }
            }
            return true;
        } 

        private static ImmutableArray<int> GetShape(ndarray<T> array, object[] indices) {
            var result = new int[array.shape.Length];

            for (var i = 0; i < indices.Length; i++) {
                switch (indices[i]) {
                    case Index index:
                        result[i] = 1;
                        break;
                    case Range range:
                        result[i] = range.GetOffsetAndLength(array.shape[i]).Length;
                        break;
                }
            }

            for (var i = indices.Length; i < array.shape.Length; i++) {
                result[i] = array.shape[i];
            }
            return result.ToImmutableArray();
        }
        #endregion
    }
}
