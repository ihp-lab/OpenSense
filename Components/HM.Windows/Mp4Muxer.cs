#nullable enable

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading;
using HMInterop;
using Microsoft.Extensions.Logging;
using Microsoft.Psi;
using Microsoft.Psi.Components;
using Minimp4Interop;

namespace OpenSense.Components.HM {
    /// <summary>
    /// Writes HEVC AccessUnit messages to an MP4 file.
    /// Input must use a non-dropping DeliveryPolicy; dropping frames causes file corruption.
    /// </summary>
    public sealed class Mp4Muxer : IConsumer<Shared<AccessUnit>>, ISourceComponent, INotifyPropertyChanged, IDisposable {

        private readonly CancellationTokenSource _cts = new();
        private readonly Queue<Shared<AccessUnit>> _pendingAccessUnits = new();

        #region Ports
        public Receiver<Shared<AccessUnit>> In { get; }
        #endregion

        #region Options
        private string filename = "video.mp4";

        public string Filename {
            get => filename;
            set => SetProperty(ref filename, value);
        }

        private bool timestampFilename;

        public bool TimestampFilename {
            get => timestampFilename;
            set => SetProperty(ref timestampFilename, value);
        }

        private bool processRemainingBeforeStop;

        public bool ProcessRemainingBeforeStop {
            get => processRemainingBeforeStop;
            set => SetProperty(ref processRemainingBeforeStop, value);
        }

        private ILogger? logger;

        public ILogger? Logger {
            get => logger;
            set => SetProperty(ref logger, value);
        }
        #endregion

        public string? ActualFilename { get; private set; }

        private DateTime prevEnvelopeTime;
        private DateTime? lastEnvelopeTime;
        private Mp4MuxerContext? context;
        private DateTime? stopFinalTime;
        private Action? stopNotifyCompleted;

        public Mp4Muxer(Pipeline pipeline) {
            In = pipeline.CreateReceiver<Shared<AccessUnit>>(this, Process, nameof(In));
        }

        #region ISourceComponent
        void ISourceComponent.Start(Action<DateTime> notifyCompletionTime) {
            notifyCompletionTime(DateTime.MaxValue);
        }

        void ISourceComponent.Stop(DateTime finalOriginatingTime, Action notifyCompleted) {
            stopFinalTime = finalOriginatingTime;
            stopNotifyCompleted = notifyCompleted;
            if (!ProcessRemainingBeforeStop) {
                _cts.Cancel();
            }
            if (!ProcessRemainingBeforeStop || lastEnvelopeTime >= finalOriginatingTime) {
                FinalizeAndComplete();
            }
        }
        #endregion

        private void Process(Shared<AccessUnit> accessUnit, Envelope envelope) {
            if (_cts.IsCancellationRequested) {
                return;
            }

            var au = accessUnit.Resource;
            EnsureContext(au, envelope.OriginatingTime);

            // Write previous AU now that we know the duration (from current AU's DTS - previous DTS)
            WritePendingAccessUnits(envelope.OriginatingTime);

            // Buffer current AU (AddRef to prevent disposal when Process returns)
            _pendingAccessUnits.Enqueue(accessUnit.AddRef());
            prevEnvelopeTime = envelope.OriginatingTime;
            lastEnvelopeTime = envelope.OriginatingTime;

            // Flush remaining when we've reached the final time
            if (envelope.OriginatingTime >= stopFinalTime) {
                FinalizeAndComplete();
            }
        }

        [MemberNotNull(nameof(context))]
        private void EnsureContext(AccessUnit firstAccessUnit, DateTime originatingTime) {
            if (context is not null) {
                return;
            }

            prevEnvelopeTime = originatingTime;

            if (!TimestampFilename) {
                ActualFilename = Filename;
            } else {
                var directory = Path.GetDirectoryName(Filename);
                var baseFilename = Path.GetFileNameWithoutExtension(Filename);
                var timestamp = originatingTime.ToString("yyyyMMddHHmmssfffffff");
                var extension = Path.GetExtension(Filename);
                var newFilename = $"{baseFilename}_{timestamp}{extension}";
                ActualFilename = Path.Combine(directory ?? string.Empty, newFilename);
            }

            if (!SequenceParameterSetParser.TryGetDimensionsFromAccessUnit(firstAccessUnit, out var w, out var h)) {
                throw new InvalidOperationException("Failed to parse SPS from the first AccessUnit. Cannot determine video dimensions.");
            }
            var stream = new FileStream(ActualFilename, FileMode.Create, FileAccess.Write, FileShare.Read);
            var muxer = new Muxer(stream, MuxMode.Default);
            var writer = new H26xWriter(muxer, w, h, isHEVC: true);
            context = new Mp4MuxerContext(stream, muxer, writer);
        }

        private void WritePendingAccessUnits(DateTime currentEnvelopeTime) {
            while (_pendingAccessUnits.Count > 0) {
                using var shared = _pendingAccessUnits.Dequeue();
                var au = shared.Resource;

                var durationTicks = (currentEnvelopeTime - prevEnvelopeTime).Ticks;
                var duration90kHz = durationTicks > 0 ? (uint)(durationTicks * 9 / 1000) : 1u;

                var ctsOffsetTicks = au.PresentationTimeOffset - au.DecodingTimeOffset;
                var ctsOffset90kHz = (int)(ctsOffsetTicks * 9 / 1000);

                var annexB = au.AnnexB;
                using var handle = annexB.Pin();
                unsafe {
                    context!.Writer.WriteNal(new IntPtr(handle.Pointer), annexB.Length, duration90kHz, ctsOffset90kHz);
                }
            }
        }

        private void FinalizeAndComplete() {
            while (_pendingAccessUnits.Count > 0) {
                using var shared = _pendingAccessUnits.Dequeue();
                var au = shared.Resource;

                var ctsOffsetTicks = au.PresentationTimeOffset - au.DecodingTimeOffset;
                var ctsOffset90kHz = (int)(ctsOffsetTicks * 9 / 1000);

                var annexB = au.AnnexB;
                using var handle = annexB.Pin();
                unsafe {
                    context!.Writer.WriteNal(new IntPtr(handle.Pointer), annexB.Length, 1, ctsOffset90kHz);
                }
            }
            context?.Dispose();
            context = null;
            stopNotifyCompleted?.Invoke();
            stopNotifyCompleted = null;
        }

        #region IDisposable
        private bool disposed;

        public void Dispose() {
            if (disposed) {
                return;
            }
            disposed = true;

            _cts.Dispose();

            while (_pendingAccessUnits.Count > 0) {
                _pendingAccessUnits.Dequeue().Dispose();
            }

            context?.Dispose();
            context = null;
        }
        #endregion

        #region INotifyPropertyChanged
        public event PropertyChangedEventHandler? PropertyChanged;

        private void SetProperty<T>(ref T field, T value, [CallerMemberName] string? propertyName = null) {
            if (!EqualityComparer<T>.Default.Equals(field, value)) {
                field = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            }
        }
        #endregion
    }
}
