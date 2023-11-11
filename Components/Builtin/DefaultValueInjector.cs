#nullable enable

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Timers;
using Microsoft.Extensions.Logging;
using Microsoft.Psi;
using Microsoft.Psi.Components;
using Timer = System.Timers.Timer;

namespace OpenSense.Components.Builtin {
    public sealed class DefaultValueInjector<T> : IConsumer<T>, IProducer<T?>, ISourceComponent, IDisposable, INotifyPropertyChanged {
        /* NOTE:
         * For rules of T? in this class, see https://learn.microsoft.com/en-us/dotnet/csharp/nullable-references#generics
         */

        private static readonly TimeSpan MinInterval = TimeSpan.FromMilliseconds(1);//psi's limitation

        private static readonly TimeSpan DefaultInputAbsenceTolerance = TimeSpan.FromMilliseconds(2 * 1000 / 30);//Two image frames

        private static readonly TimeSpan DefaultReferenceAbsenceTolerance = TimeSpan.FromMilliseconds(2 * 1000 / 30);//Two image frames

        private static readonly TimeSpan DefaultStoppingTimeout = TimeSpan.FromSeconds(1);

        private readonly object _lock = new ();

        private readonly SortedDictionary<DateTime, DateTimeOffset> _reference = new();

        private readonly SortedDictionary<DateTime, (DateTimeOffset, T)> _data = new();

        //Not using System.Threading.Timer because it is not thread-safe.
        private readonly Timer _timer = new Timer() {
            AutoReset = false,
            Interval = Math.Min(DefaultInputAbsenceTolerance.TotalMilliseconds, DefaultReferenceAbsenceTolerance.TotalMilliseconds),
        };

        private readonly Timer _stoppingTimer = new Timer() {
            AutoReset = false,
            Interval = DefaultStoppingTimeout.TotalMilliseconds,
        };

        #region Ports
        public Receiver<object?> ReferenceIn { get; }

        public Receiver<T> In { get; }

        public Emitter<T?> Out { get; }
        #endregion

        #region Settings

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

        private TimeSpan stoppingTimeout = TimeSpan.FromSeconds(1);

        public TimeSpan StoppingTimeout {
            get => stoppingTimeout;
            set {
                if (value <= TimeSpan.Zero) {
                    throw new ArgumentOutOfRangeException("value", $"{nameof(StoppingTimeout)} should be greater than 0.");
                }
                SetProperty(ref stoppingTimeout, value);
            }
        }

        private ILogger? logger;

        public ILogger? Logger {
            get => logger;
            set => SetProperty(ref logger, value);
        }
        #endregion

        #region Counters
        private long matched;

        private long missingInput;

        private long missingReference;

        private long lateInput;

        private long lateReference;

        private long passDeadline;

        private long nonIncreasing;
        #endregion

        private DateTime? finalTime;

        private Action? completeAction;

        private bool inputFinalReceived;

        private bool referenceFinalReceived;

        private bool stopped;

        public DefaultValueInjector(Pipeline pipeline) {
            ReferenceIn = pipeline.CreateReceiver<object?>(this, ProcessReference, nameof(ReferenceIn));
            In = pipeline.CreateReceiver<T>(this, ProcessInput, nameof(In));
            Out = pipeline.CreateEmitter<T?>(this, nameof(Out));

            _timer.Elapsed += OnTimer;
            _stoppingTimer.Elapsed += OnStoppingTimer;
            PropertyChanged += OnPropertyChanged;
        }

        #region ISourceComponent
        public void Start(Action<DateTime> notifyCompletionTime) {
            notifyCompletionTime(DateTime.MaxValue);
        }

        public void Stop(DateTime finalOriginatingTime, Action notifyCompleted) {
            finalTime = finalOriginatingTime.ToUniversalTime();
            completeAction = notifyCompleted;
            lock (_lock) {
                if (stopped) {
                    return;
                }
                _stoppingTimer.Start();
            }
        }
        #endregion

        #region Input Handlers
        private void ProcessReference(object? value, Envelope envelope) {
            Debug.Assert(envelope.OriginatingTime.Kind == DateTimeKind.Utc);
            var now = DateTimeOffset.UtcNow;
            lock (_lock) {
                var time = envelope.OriginatingTime.ToUniversalTime();
                if (finalTime is DateTime f && time >= f) {
                    referenceFinalReceived = true;
                }
                if (time - Out.LastEnvelope.OriginatingTime < MinInterval) {
                    lateReference++;
                    return;//This data come late, we can not reference it. Discard it.
                }
                _reference.Add(time, now);
                Process(inputFinalReceived && referenceFinalReceived);
            }
        }

        private void ProcessInput(T value, Envelope envelope) {
            Debug.Assert(envelope.OriginatingTime.Kind == DateTimeKind.Utc);
            var now = DateTimeOffset.UtcNow;
            lock (_lock) {
                var time = envelope.OriginatingTime.ToUniversalTime();
                if (finalTime is DateTime f && time >= f) {
                    inputFinalReceived = true;
                }
                if (time - Out.LastEnvelope.OriginatingTime < MinInterval) {
                    lateInput++;
                    return;//This data is replaced by the default value. Discard it.
                }
                _data.Add(time, (now, value.DeepClone()));
                Process(inputFinalReceived && referenceFinalReceived);
            }
        } 
        #endregion

        #region INotifyPropertyChanged Event handlers
        private void OnPropertyChanged(object? sender, PropertyChangedEventArgs args) {
            switch (args.PropertyName) {
                case nameof(ReferenceAbsenceTolerance):
                case nameof(InputAbsenceTolerance):
                    var interval = Math.Max(0, Math.Min(int.MaxValue, Math.Min(InputAbsenceTolerance.TotalMilliseconds, ReferenceAbsenceTolerance.TotalMilliseconds)));
                    _timer.Interval = interval;//If timer is running, it will be reset.
                    break;
                case nameof(StoppingTimeout):
                    _stoppingTimer.Interval = Math.Max(0, Math.Min(int.MaxValue, StoppingTimeout.TotalMilliseconds));//If timer is running, it will be reset.
                    break;
            }
        }
        #endregion

        #region Timer Event Handlers
        private void OnTimer(object sender, ElapsedEventArgs e) {
            try {
                lock (_lock) {
                    Process(inputFinalReceived && referenceFinalReceived);
                }
            } catch (Exception ex) {
                Logger?.LogError(ex, "");
            }
        } 

        private void OnStoppingTimer(object sender, ElapsedEventArgs e) {
            try {
                lock (_lock) {
                    Process(stopping: true);
                }
            } catch (Exception ex) {
                Logger?.LogError(ex, "");
            }
        }
        #endregion

        private void Process(bool stopping = false) {
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
                        missingReference++;
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
                    matched++;
                    GuardedPost(data, time);
                    continue;
                }

                /* Post data for non-occurred reference data. */
                if (stopping || now - recordTime > InputAbsenceTolerance) {
                    var data = DefaultValue.DeepClone();
                    _reference.Remove(time);
                    missingInput++;
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
                if (stopping || now - recordTime > ReferenceAbsenceTolerance) {
                    _data.Remove(time);
                    missingReference++;
                    GuardedPost(data, time);
                    continue;
                }
                break;
            }

            if (stopped) {
                return;
            }
            if (!stopping) {
                /* Set timer to do it again */
                if (_reference.Count != 0 || _data.Count != 0) {
                    _timer.Start();
                }
                return;
            }
            stopped = true;
            Logger?.LogInformation("DefaultValueInjector statistics: Matched {matched}, Missing Input {missingInput}, Missing Reference {missingReference}, Late Input {lateInput}, Late Reference {lateReference}, Pass Deadline {passDeadline}, Non-Increasing {nonIncreasing}.", matched, missingInput, missingReference, lateInput, lateReference, passDeadline, nonIncreasing);
            _timer.Stop();
            _stoppingTimer.Stop();
            /* Notify */
            if (completeAction is not null) {
                completeAction();
                completeAction = null;
            }
        }

        private void GuardedPost(T? data, DateTime time) {
            if (finalTime is DateTime f && time > f) {
                passDeadline++;
                return;//discard
            }
            if (time - Out.LastEnvelope.OriginatingTime < MinInterval) {
                nonIncreasing++;
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
