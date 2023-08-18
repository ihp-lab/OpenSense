#nullable enable

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Timers;
using Microsoft.Psi;

namespace OpenSense.Components.Builtin {
    public sealed class DefaultValueInjector<T> : IConsumer<T>, IProducer<T?>, IDisposable, INotifyPropertyChanged {
        /* NOTE:
         * For rules of T? in this class, see https://learn.microsoft.com/en-us/dotnet/csharp/nullable-references#generics
         */

        private static readonly TimeSpan MinInterval = TimeSpan.FromMilliseconds(1);//psi's limitation

        private static readonly TimeSpan DefaultInputAbsenceTolerance = TimeSpan.FromMilliseconds(2 * 1000 / 30);//Two image frames

        private static readonly TimeSpan DefaultReferenceAbsenceTolerance = TimeSpan.FromMilliseconds(2 * 1000 / 30);//Two image frames

        private readonly object _lock = new ();

        private readonly SortedDictionary<DateTime, DateTimeOffset> _reference = new();

        private readonly SortedDictionary<DateTime, (DateTimeOffset, T)> _data = new();

        //Not using System.Threading.Timer because it is not thread-safe.
        private readonly Timer _timer = new Timer() {
            AutoReset = false,
            Interval = Math.Min(DefaultInputAbsenceTolerance.TotalMilliseconds, DefaultReferenceAbsenceTolerance.TotalMilliseconds),
        };

        public Receiver<object?> ReferenceIn { get; }

        public Receiver<T> In { get; }

        public Emitter<T?> Out { get; }

        private T? defaultValue = default;

        public T? DefaultValue {
            get => defaultValue;
            set => SetProperty(ref defaultValue, value);
        }

        private TimeSpan inputAbsenceTolerance = DefaultInputAbsenceTolerance;

        public TimeSpan InputAbsenceTolerance {
            get => inputAbsenceTolerance;
            set {
                if (value <= TimeSpan.Zero) {
                    throw new ArgumentOutOfRangeException("value", $"{nameof(InputAbsenceTolerance)} should be greater than 0.");
                }
                SetProperty(ref inputAbsenceTolerance, value);
            }
        }

        private TimeSpan referenceAbsenceTolerance = DefaultReferenceAbsenceTolerance;

        public TimeSpan ReferenceAbsenceTolerance {
            get => referenceAbsenceTolerance;
            set {
                if (value <= TimeSpan.Zero) {
                    throw new ArgumentOutOfRangeException("value", $"{nameof(ReferenceAbsenceTolerance)} should be greater than 0.");
                }
                SetProperty(ref referenceAbsenceTolerance, value);
            }
        }

        public DefaultValueInjector(Pipeline pipeline) {
            ReferenceIn = pipeline.CreateReceiver<object?>(this, ProcessReference, nameof(ReferenceIn));
            In = pipeline.CreateReceiver<T>(this, ProcessInput, nameof(In));
            Out = pipeline.CreateEmitter<T?>(this, nameof(Out));

            _timer.Elapsed += OnTimer;
            PropertyChanged += (_, e) => {
                if (e.PropertyName == nameof(InputAbsenceTolerance)) {
                    var interval = Math.Min(InputAbsenceTolerance.TotalMilliseconds, ReferenceAbsenceTolerance.TotalMilliseconds);
                    _timer.Interval = interval;//If timer is running, it will be reset.
                }
            };

            //TODO: responsd to pipeline stop event, post remaining data.
        }

        private void ProcessReference(object? value, Envelope envelope) {
            Debug.Assert(envelope.OriginatingTime.Kind == DateTimeKind.Utc);
            var now = DateTimeOffset.UtcNow;
            lock (_lock) {
                var time = envelope.OriginatingTime.ToUniversalTime();
                if (time - Out.LastEnvelope.OriginatingTime < MinInterval) {
                    return;//This data come late, we can not reference it. Discard it.
                }
                _reference.Add(time, now);
                Process();
            }
        }

        private void ProcessInput(T value, Envelope envelope) {
            Debug.Assert(envelope.OriginatingTime.Kind == DateTimeKind.Utc);
            var now = DateTimeOffset.UtcNow;
            lock (_lock) {
                var time = envelope.OriginatingTime.ToUniversalTime();
                if (time - Out.LastEnvelope.OriginatingTime < MinInterval) {
                    return;//This data is replaced by the default value. Discard it.
                }
                _data.Add(time, (now, value.DeepClone()));
                Process();
            }
        }

        private void OnTimer(object sender, ElapsedEventArgs e) {
            lock (_lock) {
                Process();
            }
        }

        private void Process() {
            var now = DateTimeOffset.UtcNow;

            /* Post data base on the reference stream. */
            while (true) {
                if (_reference.Count == 0) {
                    break;
                }
                var kv = _reference.First();
                var (time, recordTime) = (kv.Key, kv.Value);
                Debug.Assert(time.Kind == DateTimeKind.Utc);

                /* Post all data that came before the first referce data. */
                while (true) {
                    if (_data.Count == 0) {
                        break;
                    }
                    var kvData = _data.First();
                    var (t, (_, data)) = (kvData.Key, kvData.Value);
                    if (t < time) {
                        _data.Remove(t);
                        GuardedPost(data, t);
                        continue;
                    }
                    break;
                }

                /* Post all data that has a corresponding value in the reference stream. */
                if (_data.TryGetValue(time, out var dataTuple)) {
                    var (_, data) = dataTuple;
                    _reference.Remove(time);
                    _data.Remove(time);
                    GuardedPost(data, time);
                    continue;
                }

                /* Post data for non-occurred reference data. */
                if (now - recordTime > InputAbsenceTolerance) {
                    var data = DefaultValue.DeepClone();
                    _reference.Remove(time);
                    GuardedPost(data, time);
                    continue;
                }

                /* Remaining reference data is not processable. */
                Debug.Assert(time > Out.LastEnvelope.OriginatingTime);//This condition should always be true, because late reference data was discarded.
                break;
            }

            /* Post data that waited for a long time. */
            while (true) {
                if (_data.Count == 0) {
                    break;
                }
                var kv = _data.First();
                var (time, (recordTime, data)) = (kv.Key, kv.Value);
                if (now - recordTime > ReferenceAbsenceTolerance) {
                    _data.Remove(time);
                    GuardedPost(data, time);
                    continue;
                }
                break;
            }

            /* Set timer to do it again */
            if (_reference.Count != 0 || _data.Count != 0) {
                _timer.Start();
            }
        }

        private void GuardedPost(T? data, DateTime time) {
            if (time - Out.LastEnvelope.OriginatingTime < MinInterval) {
                return;//discard
            }
            Out.Post(data, time);
            if (data is IDisposable disposable) {
                disposable.Dispose();
            }
        }

        #region IDisposable
        private bool disposed;

        public void Dispose() {
            if (disposed) {
                return;
            }
            disposed = true;

            _timer.Dispose();
        }
        #endregion

        #region INotifyPropertyChanged
        public event PropertyChangedEventHandler? PropertyChanged;

#pragma warning disable CS0693 // Type parameter has the same name as the type parameter from outer type
        private void SetProperty<T>(ref T field, T value, [CallerMemberName] string? propertyName = null) {
            if (!EqualityComparer<T>.Default.Equals(field, value)) {
                field = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            }
        }
#pragma warning restore CS0693 // Type parameter has the same name as the type parameter from outer type
        #endregion
    }
}
