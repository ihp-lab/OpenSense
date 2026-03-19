#nullable enable

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Reflection;
using System.Runtime.CompilerServices;
using HMInterop;
using Microsoft.Extensions.Logging;
using Microsoft.Psi;
using Minimp4Interop;

namespace OpenSense.Components.HM {
    /// <summary>
    /// Writes HEVC AccessUnit messages to an MP4 file.
    /// Input must use a non-dropping DeliveryPolicy; dropping frames causes file corruption.
    /// </summary>
    public sealed class Mp4Muxer : IConsumer<Shared<AccessUnit>>, INotifyPropertyChanged, IDisposable {

        /// <summary>
        /// Minimum duration in 90kHz units, used for the last frame(s) where no subsequent frame
        /// is available to compute actual duration.
        /// </summary>
        private const uint MinDuration90kHz = 1;

        private static readonly PropertyInfo IsStoppingProperty =
            typeof(Pipeline).GetProperty("IsStopping", BindingFlags.Instance | BindingFlags.NonPublic)
            ?? throw new MissingMemberException(nameof(Pipeline), "IsStopping");

        private readonly Pipeline _pipeline;
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

        private bool discardRemainingOnStop;

        public bool DiscardRemainingOnStop {
            get => discardRemainingOnStop;
            set => SetProperty(ref discardRemainingOnStop, value);
        }

        private ILogger? logger;

        public ILogger? Logger {
            get => logger;
            set => SetProperty(ref logger, value);
        }
        #endregion

        public string? ActualFilename { get; private set; }

        private DateTime prevEnvelopeTime;
        private Mp4MuxerContext? context;

        public Mp4Muxer(Pipeline pipeline) {
            _pipeline = pipeline;
            In = pipeline.CreateReceiver<Shared<AccessUnit>>(this, Process, nameof(In));
            pipeline.PipelineCompleted += OnPipelineCompleted;
        }

        #region Pipeline Event Handlers
        private void OnPipelineCompleted(object? sender, PipelineCompletedEventArgs args) {
            WritePendingAccessUnits(currentEnvelopeTime: null);
            context?.Dispose();
            context = null;
        } 
        #endregion

        private void Process(Shared<AccessUnit> accessUnit, Envelope envelope) {
            if (DiscardRemainingOnStop && (bool)IsStoppingProperty.GetValue(_pipeline)!) {
                return;
            }

            var au = accessUnit.Resource;
            EnsureContext(au, envelope.OriginatingTime);

            // Write previous AU now that we know the duration (from current AU's DTS - previous DTS)
            WritePendingAccessUnits(envelope.OriginatingTime);

            // Buffer current AU (AddRef to prevent disposal when Process returns)
            _pendingAccessUnits.Enqueue(accessUnit.AddRef());
            prevEnvelopeTime = envelope.OriginatingTime;
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

        /// <summary>
        /// Write all pending access units to the MP4 file.
        /// When currentEnvelopeTime is provided, duration is computed from the time difference.
        /// When null (pipeline completed), MinDuration is used for remaining frames.
        /// </summary>
        private void WritePendingAccessUnits(DateTime? currentEnvelopeTime) {
            while (_pendingAccessUnits.Count > 0) {
                using var shared = _pendingAccessUnits.Dequeue();
                var au = shared.Resource;

                var duration90kHz = MinDuration90kHz;
                if (currentEnvelopeTime is { } time) {
                    var durationTicks = (time - prevEnvelopeTime).Ticks;
                    if (durationTicks > 0) {
                        duration90kHz = (uint)(durationTicks * 9 / 1000);
                    }
                }

                var ctsOffsetTicks = au.PresentationTimeOffset - au.DecodingTimeOffset;
                var ctsOffset90kHz = (int)(ctsOffsetTicks * 9 / 1000);

                var annexB = au.AnnexB;
                using var handle = annexB.Pin();
                unsafe {
                    context!.Writer.WriteNal(new IntPtr(handle.Pointer), annexB.Length, duration90kHz, ctsOffset90kHz);
                }
            }
        }

        #region IDisposable
        private bool disposed;

        public void Dispose() {
            if (disposed) {
                return;
            }
            disposed = true;

            while (_pendingAccessUnits.Count > 0) {
                _pendingAccessUnits.Dequeue().Dispose();
            }
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
