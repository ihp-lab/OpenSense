using System;
using System.IO;
using KvazaarInterop;
using Minimp4Interop;

namespace OpenSense.Components.Kvazaar {
    internal sealed record class FileWriterContext(
        DateTime StartTime,
        Config Config,
        Encoder Encoder,
        FileStream Stream,
        Muxer Muxer,
        H26xWriter Writer
    ) : IDisposable {

        #region IDisposable
        private bool _disposed;

        public void Dispose() {
            if (_disposed) {
                return;
            }
            _disposed = true;

            Encoder.Dispose();
            Config.Dispose();

            Writer.Dispose();
            Muxer.Dispose();
            Stream.Dispose();
        } 
        #endregion
    }
}
