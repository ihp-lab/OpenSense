using System;
using System.IO;
using Minimp4Interop;

namespace OpenSense.Components.HM {
    internal sealed class Mp4MuxerContext : IDisposable {

        public FileStream Stream { get; }
        public Muxer Muxer { get; }
        public H26xWriter Writer { get; }

        public Mp4MuxerContext(FileStream stream, Muxer muxer, H26xWriter writer) {
            Stream = stream;
            Muxer = muxer;
            Writer = writer;
        }

        #region IDisposable
        private bool disposed;

        public void Dispose() {
            if (disposed) {
                return;
            }
            disposed = true;

            Writer.Dispose();
            Muxer.Dispose();
            Stream.Dispose();
        }
        #endregion
    }
}
