using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using Microsoft.Extensions.Logging;
using Microsoft.Psi;
using OpenSense.VHMessage;

namespace OpenSense.Components.VHMessage {
    /// <remarks>
    /// This object will not be disposed by the pipeline, it must be disposed manually.
    /// </remarks>
    public sealed class VHMessageTransceiver : INotifyPropertyChanged, IDisposable {

        private readonly VirtualHumanMessageClient _client;

        private readonly Pipeline _pipeline;

        private readonly Dictionary<string, Subscription> _subscriptions = new Dictionary<string, Subscription>();

        private readonly Dictionary<string, Subject> _subjects = new Dictionary<string, Subject>();

        public ILogger? Logger { get; set; }

        private bool started;

        public VHMessageTransceiver(Pipeline pipeline) {
            _client = new VirtualHumanMessageClient();
            _pipeline = pipeline;

            pipeline.PipelineRun += OnPipelineRun;
        }

        public Receiver<string?> AddInput(string scope) {
            if (scope is null) {
                throw new ArgumentNullException(nameof(scope));
            }
            if (started) {
                throw new InvalidOperationException();
            }
            var s = new Subject(scope, _pipeline, _client);
            _subjects.Add(scope, s);
            return s.Receiver;
        }

        public Emitter<string> AddOutput(string scope) {
            if (scope is null) {
                throw new ArgumentNullException(nameof(scope));
            }
            if (started) {
                throw new InvalidOperationException();
            }
            var s = new Subscription(scope, _pipeline, _client);
            _subscriptions.Add(scope, s);
            return s.Emitter;
        }

        #region Pipeline Event Handlers
        private void OnPipelineRun(object? sender, PipelineRunEventArgs args) {
            started = true;
            _client.OpenConnection();
            foreach (var s in _subjects.Values) {
                s.Ready();
            }
            foreach (var s in _subscriptions.Values) {
                s.Subscribe();
            }
        }
        #endregion

        #region IDisposable
        private bool disposed;

        public void Dispose() {
            if (disposed) {
                return;
            }
            disposed = true;

            _client.Dispose();
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

        #region Classes
        /// <summary>
        /// For receiving
        /// </summary>
        private sealed class Subscription {

            private static readonly TimeSpan MinInterval = TimeSpan.FromMilliseconds(1);

            private static readonly char[] Seperators = new[] { ' ', };

            private readonly VirtualHumanMessageClient _client;

            private readonly bool _noSubject;

            public string Name { get; }

            public Emitter<string> Emitter { get; }

            public Subscription(string name, Pipeline pipeline, VirtualHumanMessageClient client) {
                _client = client;
                Name = name.Trim();
                _noSubject = string.IsNullOrEmpty(Name);
                Emitter = pipeline.CreateEmitter<string>(this, name);
            }

            private void OnMessage(object? sender, NMS.ActiveMQ.Message message) {
                string text;
                if (_noSubject) {
                    text = message.TextMessage;
                } else {
                    var splits = message.TextMessage.Split(Seperators, 2, StringSplitOptions.RemoveEmptyEntries);
                    if (splits.Length == 0) {
                        return;
                    }
                    if (splits[0] != Name) {
                        return;
                    }
                    text = splits.Length == 1 ? string.Empty : splits[1];
                }
                text = text.Trim();
                var time = DateTime.UtcNow;
                var safeTime = Emitter.LastEnvelope.OriginatingTime + MinInterval;
                if (time < safeTime) {
                    time = safeTime;
                }
                Emitter.Post(text, time);//TODO: can we get timestamp from the message itself?
            }

            public void Subscribe() {
                _client.Subscribe(OnMessage);
            }
        }

        /// <summary>
        /// For sending
        /// </summary>
        private sealed class Subject {

            private readonly VirtualHumanMessageClient _client;

            private readonly bool _noSubject;

            public string Name { get; }

            public Receiver<string?> Receiver { get; }

            private bool connected;

            public Subject(string name, Pipeline pipeline, VirtualHumanMessageClient client) {
                _client = client;
                Name = name.Trim();
                _noSubject = string.IsNullOrEmpty(Name);
                Receiver = pipeline.CreateReceiver<string?>(this, Process, Name);
            }

            private void Process(string? data, Envelope envelope) {
                Debug.Assert(connected);
                var text = _noSubject ?
                    data : $"{Name} {data}";
                text = text?.Trim();
                if (string.IsNullOrEmpty(text)) {
                    return;
                }
                _client.SendMessage(text!);
            }

            public void Ready() {
                connected = true;
            }
        }
        #endregion
    }
}
