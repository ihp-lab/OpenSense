#nullable enable

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Threading;
using HMInterop;
using Microsoft.Extensions.Logging;
using Microsoft.Psi;
using Microsoft.Psi.Components;

namespace OpenSense.Components.HM {
    /// <summary>
    /// Encodes Picture frames into HEVC AccessUnit messages using HM encoder.
    /// Due to HM limitations, all HM components are serialized at runtime.
    /// </summary>
    public sealed class HevcEncoder : IConsumer<Shared<Picture>>, IProducer<Shared<AccessUnit>>, ISourceComponent, INotifyPropertyChanged, IDisposable {

        private readonly CancellationTokenSource _cts = new();
        private readonly List<AccessUnit> _encodedUnits = new();
        private readonly Queue<long> _dtsQueue = new();

        private DateTime? baseTime;

        #region Ports
        public Receiver<Shared<Picture>> In { get; }

        public Emitter<Shared<AccessUnit>> Out { get; }
        #endregion

        #region Options
        private int? inputBitDepth;

        /// <summary>
        /// Expected input bit depth. Null = auto (detect from input at runtime). Set to validate input.
        /// </summary>
        public int? InputBitDepth {
            get => inputBitDepth;
            set => SetProperty(ref inputBitDepth, value);
        }

        private ChromaFormat? inputChromaFormat;

        /// <summary>
        /// Expected input chroma format. Null = auto (detect from input at runtime). Set to validate input.
        /// </summary>
        public ChromaFormat? InputChromaFormat {
            get => inputChromaFormat;
            set => SetProperty(ref inputChromaFormat, value);
        }

        private int? internalBitDepth;

        /// <summary>
        /// Internal bit depth for encoding. Null = same as input bit depth.
        /// </summary>
        public int? InternalBitDepth { 
            get => internalBitDepth;
            set => SetProperty(ref internalBitDepth, value);
        }

        private EncoderConfig encoderConfiguration = new EncoderConfig();

        public EncoderConfig EncoderConfiguration {
            get => encoderConfiguration;
            set => SetProperty(ref encoderConfiguration, value);
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

        private Encoder? encoder;
        private DateTime? lastEnvelopeTime;
        private DateTime? stopFinalTime;
        private Action? stopNotifyCompleted;

        public HevcEncoder(Pipeline pipeline) {
            In = pipeline.CreateReceiver<Shared<Picture>>(this, Process, nameof(In));
            Out = pipeline.CreateEmitter<Shared<AccessUnit>>(this, nameof(Out));
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
                FlushAndComplete();
            }
        }
        #endregion

        private void Process(Shared<Picture> picture, Envelope envelope) {
            if (_cts.IsCancellationRequested) {
                return;
            }

            var picYuv = picture.Resource.PicYuv;
            EnsureEncoder(picYuv.Width, picYuv.Height, picYuv.ChromaFormat, picture.Resource.Sps.BitDepths.Luma);

            baseTime ??= envelope.OriginatingTime;

            _dtsQueue.Enqueue((envelope.OriginatingTime - baseTime.Value).Ticks);

            _encodedUnits.Clear();
            encoder!.Encode(picYuv, (envelope.OriginatingTime - baseTime.Value).Ticks, _encodedUnits);
            PostEncodedUnits();
            lastEnvelopeTime = envelope.OriginatingTime;

            // Flush remaining GOP-buffered frames when we've reached the final time
            if (envelope.OriginatingTime >= stopFinalTime) {
                FlushAndComplete();
            }
        }

        [MemberNotNull(nameof(encoder))]
        private void EnsureEncoder(int width, int height, ChromaFormat chromaFmt, int bitDepth) {
            if (encoder is not null) {
                return;
            }

            if (InputBitDepth is { } expectedBitDepth && expectedBitDepth != bitDepth) {
                throw new InvalidOperationException($"InputBitDepth mismatch: configured {expectedBitDepth}, but input Picture has {bitDepth}-bit data.");
            }
            if (InputChromaFormat is { } expectedChromaFormat && expectedChromaFormat != chromaFmt) {
                throw new InvalidOperationException($"InputChromaFormat mismatch: configured {expectedChromaFormat}, but input Picture has {chromaFmt}.");
            }

            var config = EncoderConfiguration;
            config.SourceWidth = width;
            config.SourceHeight = height;
            config.InputBitDepth = bitDepth;
            config.InternalBitDepth = InternalBitDepth ?? bitDepth;
            config.ChromaFormatIdc = chromaFmt;
            encoder = new Encoder(config);
        }
        private void PostEncodedUnits() {
            foreach (var au in _encodedUnits) {
                // Use queued input times (FIFO) for monotonically increasing DTS.
                // au.DecodingTimeOffset is PTS (not true DTS), which is non-monotonic in coding order.
                var dts = _dtsQueue.Dequeue();
                var originatingTime = baseTime!.Value + TimeSpan.FromTicks(dts);
                using var shared = Shared.Create(au);
                Out.Post(shared, originatingTime);
            }
        }

        private void FlushAndComplete() {
            if (encoder is not null) {
                _encodedUnits.Clear();
                encoder.Flush(_encodedUnits);
                PostEncodedUnits();
            }
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
            encoder?.Dispose();
            encoder = null;
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
