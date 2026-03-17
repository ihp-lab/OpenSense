using System;
using System.IO;
using Minimp4Interop;

namespace OpenSense.Components.HM {
    internal sealed class Mp4DemuxerContext : IDisposable {

        public DateTime StartTime { get; }
        public FileStream Stream { get; }
        public Demuxer Demuxer { get; }
        public int VideoTrackIndex { get; }
        public uint Timescale { get; }

        public Mp4DemuxerContext(DateTime startTime, FileStream stream, Demuxer demuxer, int videoTrackIndex, uint timescale) {
            StartTime = startTime;
            Stream = stream;
            Demuxer = demuxer;
            VideoTrackIndex = videoTrackIndex;
            Timescale = timescale;
        }

        #region IDisposable
        private bool disposed;

        public void Dispose() {
            if (disposed) {
                return;
            }
            disposed = true;

            Demuxer.Dispose();
            Stream.Dispose();
        }
        #endregion
    }
}
