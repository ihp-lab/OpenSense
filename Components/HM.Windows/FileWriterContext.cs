using System;
using System.IO;
using HMInterop;
using Minimp4Interop;

namespace OpenSense.Components.HM {
    internal sealed record class FileWriterContext(
        DateTime StartTime,
        EncoderConfig Config,
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

            Writer.Dispose();
            Muxer.Dispose();
            Stream.Dispose();
        }
        #endregion
    }
}
