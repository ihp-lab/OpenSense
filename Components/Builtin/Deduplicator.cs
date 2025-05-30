﻿#nullable enable

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Microsoft.Psi;

namespace OpenSense.Components.Builtin {
    public sealed class Deduplicator<T> : IConsumer<T>, IProducer<T>, INotifyPropertyChanged {
        #region Settings
        private IEqualityComparer<T> comparer = EqualityComparer<T>.Default;

        public IEqualityComparer<T> Comparer {
            get => comparer;
            set => SetProperty(ref comparer, value);
        }

        private TimeSpan dullness = TimeSpan.Zero;

        public TimeSpan Dullness {
            get => dullness;
            set => SetProperty(ref dullness, value);
        }

        private TimeSpan expiration = TimeSpan.MaxValue;

        public TimeSpan Expiration {
            get => expiration;
            set => SetProperty(ref expiration, value);
        }
        #endregion

        #region Ports
        public Receiver<T> In { get; }

        public Emitter<T> Out { get; }
        #endregion

        private bool hasFirstValue;

        private T lastValue = default!;

        private DateTime lastTime;

        public Deduplicator(Pipeline pipeline) {
            In = pipeline.CreateReceiver<T>(this, Process, nameof(In));
            Out = pipeline.CreateEmitter<T>(this, nameof(Out));
        }

        private void Process(T value, Envelope envelope) {
            if (!hasFirstValue) {
                hasFirstValue = true;
                Out.Post(value, envelope.OriginatingTime);
                lastValue = value.DeepClone();
                lastTime = envelope.OriginatingTime;
                return;
            }

            if (envelope.OriginatingTime - lastTime > Dullness) {
                if (!Comparer.Equals(value, lastValue)) {
                    Out.Post(value, envelope.OriginatingTime);
                    lastValue = value.DeepClone();
                    lastTime = envelope.OriginatingTime;
                    return;
                }

                if (envelope.OriginatingTime - lastTime > Expiration) {
                    Out.Post(value, envelope.OriginatingTime);
                    lastTime = envelope.OriginatingTime;
                    return;
                }
            }
        }

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
