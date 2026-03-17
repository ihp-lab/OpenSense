#nullable enable

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading;
using HMInterop;
using Microsoft.Extensions.Logging;
using Microsoft.Psi;
using Microsoft.Psi.Components;

namespace OpenSense.Components.HM {
    /// <summary>
    /// Decodes HEVC AccessUnit messages into Picture frames using HM decoder.
    /// Input must use a non-dropping DeliveryPolicy; dropping frames causes decoding errors.
    /// </summary>
    public sealed class HevcDecoder : IConsumer<Shared<AccessUnit>>, IProducer<Shared<Picture>>, ISourceComponent, INotifyPropertyChanged, IDisposable {

        private readonly Decoder _decoder = new();
        private readonly CancellationTokenSource _cts = new();
        private readonly PriorityQueue<long, long> _ptsQueue = new();
        private readonly List<Picture> _decodedFrames = new();

        #region Ports
        public Receiver<Shared<AccessUnit>> In { get; }

        public Emitter<Shared<Picture>> Out { get; }
        #endregion

        #region Options
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

        public SequenceParameterSet? SequenceParameterSet { get; private set; }

        private DateTime? baseTime;
        private DateTime? lastEnvelopeTime;
        private DateTime? stopFinalTime;
        private Action? stopNotifyCompleted;

        public HevcDecoder(Pipeline pipeline) {
            In = pipeline.CreateReceiver<Shared<AccessUnit>>(this, Process, nameof(In));
            Out = pipeline.CreateEmitter<Shared<Picture>>(this, nameof(Out));
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

        private void Process(Shared<AccessUnit> accessUnit, Envelope envelope) {
            if (_cts.IsCancellationRequested) {
                return;
            }

            var au = accessUnit.Resource;
            baseTime ??= envelope.OriginatingTime;

            // Record PTS for display-order matching
            var pts = au.PresentationTimeOffset;
            _ptsQueue.Enqueue(pts, pts);

            // Feed all NALs to the decoder in a single pin
            _decodedFrames.Clear();
            _decoder.FeedAccessUnit(au, _decodedFrames);

            // Post decoded frames with correct display timestamps
            PostDecodedFrames();
            lastEnvelopeTime = envelope.OriginatingTime;

            // Flush remaining B-frames when we've reached the final time
            if (envelope.OriginatingTime >= stopFinalTime) {
                FlushAndComplete();
            }
        }

        private void PostDecodedFrames() {
            foreach (var pic in _decodedFrames) {
                SequenceParameterSet ??= pic.Sps;

                var pts = _ptsQueue.Dequeue();
                var originatingTime = (DateTime)baseTime! + TimeSpan.FromTicks(pts);

                using var shared = Shared.Create(pic);
                Out.Post(shared, originatingTime);
            }
        }

        private void FlushAndComplete() {
            _decodedFrames.Clear();
            _decoder.FlushAndCollect(_decodedFrames);
            PostDecodedFrames();
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
            _decoder.Dispose();
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
