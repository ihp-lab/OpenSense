using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using Microsoft.Psi;
using Microsoft.Psi.Components;

namespace OpenSense.Components.PortableFACS.Visualizer {
    public sealed class ActionUnitVisualizer : IConsumerProducer<IReadOnlyDictionary<int, float>, string>, INotifyPropertyChanged {

        private readonly StringBuilder _stringBuilder = new StringBuilder();

        #region Ports
        public Receiver<IReadOnlyDictionary<int, float>> In { get; }

        public Emitter<string> Out { get; }
        #endregion

        private string text;

        public string Text {
            get => text;
            set => SetProperty(ref text, value);
        }

        public ActionUnitVisualizer(Pipeline pipeline) {
            In = pipeline.CreateReceiver<IReadOnlyDictionary<int, float>>(this, Process, nameof(In));
            Out = pipeline.CreateEmitter<string>(this, nameof(Out));
        }

        private void Process(IReadOnlyDictionary<int, float> actionUnits, Envelope envelope) {
            _stringBuilder.Clear();
            foreach (var kv in actionUnits.OrderBy(p => p.Key)) {
                var keyStr = kv.Key.ToString();
                keyStr = keyStr.PadLeft(2, ' ');
                var valueStr = kv.Value.ToString("F3");
                valueStr = valueStr.PadLeft(7, ' ');
                _stringBuilder.Append(keyStr);
                _stringBuilder.Append(":\t");
                _stringBuilder.Append(valueStr);
                _stringBuilder.AppendLine();
            }
            Text = _stringBuilder.ToString();
            Out.Post(Text, envelope.OriginatingTime);
        }

        #region INotifyPropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;

        private void SetProperty<T>(ref T field, T value, [CallerMemberName] string propertyName = null) {
            if (!EqualityComparer<T>.Default.Equals(field, value)) {
                field = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            }
        }
        #endregion
    }
}
