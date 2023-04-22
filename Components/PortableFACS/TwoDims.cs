using System;
using System.Buffers;
using System.Diagnostics;
using System.Text;

namespace OpenSense.Components.PortableFACS {
    internal sealed class TwoDims<T>: IDisposable
        where T: struct 
    {

        private readonly T[] data;

        public int Rows { get; }

        public int Columns { get; }

        public int Length { get; } 

        public T this[int row, int column] {
            get => data[GetIndex(row, column)];
            set => data[GetIndex(row, column)] = value;
        }

        public TwoDims(int rows, int columns) {
            Debug.Assert(rows > 0);
            Debug.Assert(columns > 0);
            Rows = rows;
            Columns = columns;
            Length = rows * columns;
            data = ArrayPool<T>.Shared.Rent(Length);
        }

        public ref T GetRef(int row, int column) => ref data[GetIndex(row, column)];

        private int GetIndex(int row, int column) {
            Debug.Assert(0 <= row && row < Rows);
            Debug.Assert(0 <= column && column < Columns);
            var result = row * Columns + column;
            return result;
        }

        #region IDisposable
        private bool disposed;

        private void Dispose(bool disposing) {
            if (disposed) {
                return;
            }

            if (disposing) {
                
            }
            ArrayPool<T>.Shared.Return(data);

            disposed = true;
        }

        ~TwoDims() {
            Dispose(disposing: false);
        }

        public void Dispose() {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
        #endregion
    }
}
