/** NOTE:
 * Nullable need to be disabled 
 */
#nullable disable

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Microsoft.Psi;
using Microsoft.Psi.Components;

namespace OpenSense.Components.Builtin {
    public class InteractiveValueSource<T> : ISourceComponent, IProducer<T>, INotifyPropertyChanged {

        private static readonly TimeSpan ClockResolution = TimeSpan.FromMilliseconds(1);

        private readonly object _lock = new();

        #region Options
        private T defaultValue;

        public T DefaultValue {
            get => defaultValue;
            set => SetProperty(ref defaultValue, value);
        }

        private bool postPipelineStartValue;

        public bool PostPipelineStartValue {
            get => postPipelineStartValue;
            set => SetProperty(ref postPipelineStartValue, value);
        }

        private T piplineStartValue;

        public T PipelineStartValue {
            get => piplineStartValue;
            set => SetProperty(ref piplineStartValue, value);
        }

        private bool postPipelineStopValue;

        public bool PostPipelineStopValue {
            get => postPipelineStopValue;
            set => SetProperty(ref postPipelineStopValue, value);
        }

        private T piplineStopValue;

        public T PipelineStopValue {
            get => piplineStopValue;
            set => SetProperty(ref piplineStopValue, value);
        }

        private IEqualityComparer<T> comparer = EqualityComparer<T>.Default;

        public IEqualityComparer<T> Comparer {
            get => comparer;
            set => SetProperty(ref comparer, value);
        }

        private bool deduplicate;

        public bool Deduplicate {
            get => deduplicate;
            set => SetProperty(ref deduplicate, value);
        }
        #endregion

        #region Ports
        public Emitter<T> Out { get; }
        #endregion

        private T currentValue;

        public T CurrentValue {
            get => currentValue;
            private set => SetProperty(ref currentValue, value);
        }

        private bool started;

        private bool completed;

        public InteractiveValueSource(Pipeline pipeline) {
            Out = pipeline.CreateEmitter<T>(this, nameof(Out));

            pipeline.PipelineRun += OnPipelineRun;
            pipeline.PipelineCompleted += OnPipelineCompleted;
        }

        #region ISourceComponent
        void ISourceComponent.Start(Action<DateTime> notifyCompletionTime) { 
            notifyCompletionTime(DateTime.MaxValue);//TODO: support interactive complete operation
        }

        void ISourceComponent.Stop(DateTime finalOriginatingTime, Action notifyCompleted) {
            lock (_lock) {
                if (postPipelineStopValue) {
                    Out.Post(piplineStopValue, finalOriginatingTime);
                }
                completed = true;
            }
        }
        #endregion

        #region Pipeline Event Handlers
        private void OnPipelineRun(object sender, PipelineRunEventArgs args) {
            lock (_lock) {
                CurrentValue = DefaultValue;
                if (PostPipelineStartValue) {
                    var startValue = PipelineStartValue;
                    SafePost(startValue, args.StartOriginatingTime);
                }
                started = true;
            }
        }

        private void OnPipelineCompleted(object sender, PipelineCompletedEventArgs args) {
            //nothing
        }
        #endregion

        #region APIs
        public void SetValue(T value) {
            lock (_lock) {
                if (!started) {
                    return;
                }
                if (completed) {
                    return;
                }
                if (Deduplicate) {
                    if (Comparer.Equals(value, CurrentValue)) {
                        return;
                    }
                }
                var now = DateTime.UtcNow;
                SafePost(value, now);
            }
        }
        #endregion

        private void SafePost(T value, DateTime originatingTime) {
            var safeTime = Out.LastEnvelope.OriginatingTime + ClockResolution;
            if (originatingTime < safeTime) {
                originatingTime = safeTime;
            }
            Out.Post(value, originatingTime);
            CurrentValue = value;
        }

        #region INotifyPropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;

#pragma warning disable CS0693 // Type parameter has the same name as the type parameter from outer type
        private void SetProperty<T>(ref T field, T value, [CallerMemberName] string propertyName = null) {
            if (!EqualityComparer<T>.Default.Equals(field, value)) {
                field = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            }
        }
#pragma warning restore CS0693 // Type parameter has the same name as the type parameter from outer type
        #endregion
    }
}
