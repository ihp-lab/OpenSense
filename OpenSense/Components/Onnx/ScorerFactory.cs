using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenSense.Components.Onnx {
    internal static class ScorerFactory {
        public static OnnxCNNModelScorer Create(string creatorName) {
            switch (creatorName) {
                case nameof(OnnxEmotionModelScorer):
                    return new OnnxEmotionModelScorer();
                case nameof(OnnxSpeechDetectionModelScorer):
                    return new OnnxSpeechDetectionModelScorer();
                default:
                    throw new InvalidOperationException();
            }
        }
    }
}
