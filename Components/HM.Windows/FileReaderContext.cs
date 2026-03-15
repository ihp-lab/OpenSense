using System;
using System.IO;
using HMInterop;
using Minimp4Interop;

namespace OpenSense.Components.HM {
    internal sealed class FileReaderContext : IDisposable {

        public DateTime StartTime { get; }
        public FileStream Stream { get; }
        public Demuxer Demuxer { get; }
        public int VideoTrackIndex { get; }
        public uint Timescale { get; }
        public Decoder Decoder { get; }

        public FileReaderContext(DateTime startTime, FileStream stream, Demuxer demuxer, int videoTrackIndex, uint timescale, Decoder decoder) {
            StartTime = startTime;
            Stream = stream;
            Demuxer = demuxer;
            VideoTrackIndex = videoTrackIndex;
            Timescale = timescale;
            Decoder = decoder;
        }

        #region IDisposable
        private bool disposed;

        public void Dispose() {
            if (disposed) {
                return;
            }
            disposed = true;

            Decoder.Dispose();
            Demuxer.Dispose();
            Stream.Dispose();
        }
        #endregion
    }
}
