using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Microsoft.Psi;
using OpenSense.Component.Emotion.Common;

namespace OpenSense.Component.Emotion.Visualizer {
    public class EmotionVisualizer: IConsumer<Emotions>, INotifyPropertyChanged {
        #region INotifyPropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;

        private void SetProperty<T>(ref T field, T value, [CallerMemberName] string propertyName = null) {
            if (!EqualityComparer<T>.Default.Equals(field, value)) {
                field = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            }
        }
        #endregion

        public Receiver<Emotions> In { get; private set; }

        public EmotionVisualizer(Pipeline pipeline) {
            In = pipeline.CreateReceiver<Emotions>(this, Process, nameof(In));
            pipeline.PipelineCompleted += PipelineCompleted;
        }

        private void Process(Emotions value, Envelope envelope) {
            Angry = value.Angry;
            Disgust = value.Disgust;
            Fear = value.Fear;
            Happy = value.Happy;
            Neutral = value.Neutral;
            Sad = value.Sad;
            Surprise = value.Surprise;
        }

        private void PipelineCompleted(object sender, PipelineCompletedEventArgs e) {
            Angry = null;
            Disgust = null;
            Fear = null;
            Happy = null;
            Neutral = null;
            Sad = null;
            Surprise = null;
        }

        private float? angry;
        public float? Angry {
            get => angry;
            set => SetProperty(ref angry, value);
        }

        private float? disgust;
        public float? Disgust {
            get => disgust;
            set => SetProperty(ref disgust, value);
        }

        private float? fear;
        public float? Fear {
            get => fear;
            set => SetProperty(ref fear, value);
        }

        private float? happy;
        public float? Happy {
            get => happy;
            set => SetProperty(ref happy, value);
        }

        private float? neutral;
        public float? Neutral {
            get => neutral;
            set => SetProperty(ref neutral, value);
        }

        private float? sad;
        public float? Sad {
            get => sad;
            set => SetProperty(ref sad, value);
        }

        private float? surprise;
        public float? Surprise {
            get => surprise;
            set => SetProperty(ref surprise, value);
        }
    }
}
