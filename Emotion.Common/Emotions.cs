using System.Collections.Generic;
using System.Diagnostics;

namespace OpenSense.Component.Emotion.Common {
    public class Emotions {

        public Emotions(IList<float> emotions) {
            Debug.Assert(emotions != null && emotions.Count == 7);
            Angry = emotions[0];
            Disgust = emotions[1];
            Fear = emotions[2];
            Happy = emotions[3];
            Neutral = emotions[4];
            Sad = emotions[5];
            Surprise = emotions[6];
        }

        public float Angry;
        public float Disgust;
        public float Fear;
        public float Happy;
        public float Neutral;
        public float Sad;
        public float Surprise;
    }
}
