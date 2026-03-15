using System;
using System.IO;
using HMInterop;
using Minimp4Interop;

namespace OpenSense.Components.HM {
    internal sealed class FileWriterContext : IDisposable {

        public DateTime StartTime { get; }
        public EncoderConfig Config { get; }
        public Encoder Encoder { get; }
        public FileStream Stream { get; }
        public Muxer Muxer { get; }
        public H26xWriter Writer { get; }

        public FileWriterContext(DateTime startTime, EncoderConfig config, Encoder encoder, FileStream stream, Muxer muxer, H26xWriter writer) {
            StartTime = startTime;
            Config = config;
            Encoder = encoder;
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

            Encoder.Dispose();

            Writer.Dispose();
            Muxer.Dispose();
            Stream.Dispose();
        }
        #endregion
    }
}
