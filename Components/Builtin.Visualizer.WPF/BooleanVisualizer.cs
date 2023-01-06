using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Microsoft.Psi;
using Microsoft.Psi.Components;

namespace OpenSense.Components.Builtin.Visualizer {
    public sealed class BooleanVisualizer : IConsumerProducer<bool?, string>, INotifyPropertyChanged {
        #region INotifyPropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;

        private void SetProperty<T>(ref T field, T value, [CallerMemberName] string propertyName = null) {
            if (!EqualityComparer<T>.Default.Equals(field, value)) {
                field = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            }
        }
        #endregion

        #region Settings
        private int historyCapacity = 100;

        public int HistoryCapacity {
            get => historyCapacity;
            set => SetProperty(ref historyCapacity, value);
        }

        private string falseText = "✕";

        public string FalseText {
            get => falseText;
            set => SetProperty(ref falseText, value);
        }

        private string trueText = "✓";

        public string TrueText {
            get => trueText;
            set => SetProperty(ref trueText, value);
        }

        private string nullText = "?";

        public string NullText {
            get => nullText;
            set => SetProperty(ref nullText, value);
        }
        #endregion

        #region Ports
        public Receiver<bool?> In { get; } 

        public Emitter<string> Out { get; }
        #endregion

        public BooleanVisualizer(Pipeline pipeline) {
            In = pipeline.CreateReceiver<bool?>(this, Process, nameof(In));
            Out = pipeline.CreateEmitter<string>(this, nameof(Out));
            pipeline.PipelineCompleted += PipelineCompleted;
        }

        private void Process(bool? value, Envelope envelope) {
            Value = value;
            var text = value switch {
                null => NullText,
                true => TrueText,
                false => FalseText,
            };
            Text = text;
            Out.Post(text, envelope.OriginatingTime);
            var history = History;
            var subLength = HistoryCapacity - text.Length;
            if (subLength > 0) {
                if (history.Length > subLength) {
                    History = text + history.Substring(startIndex: 0, length: subLength);
                } else {
                    History = text + history;
                }
            } else {
                History = text;
            }
        }

        private void PipelineCompleted(object sender, PipelineCompletedEventArgs e) {
            Value = null;
            Text = null;
            History = string.Empty;
        }

        #region Data Bindings
        private bool? val;

        public bool? Value {
            get => val;
            private set => SetProperty(ref val, value);
        }

        private string text;

        public string Text {
            get => text;
            private set => SetProperty(ref text, value);
        }

        private string history = string.Empty;

        public string History {
            get => history;
            private set => SetProperty(ref history, value);
        } 
        #endregion
    }
}
