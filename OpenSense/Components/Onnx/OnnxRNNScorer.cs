using Microsoft.ML;
using Microsoft.ML.Data;
using Microsoft.ML.Transforms.Onnx;
using System.Collections.Generic;

namespace OpenSense.Components.Onnx {
    internal class OnnxRNNScorer {
        private readonly string modelLocation;
        private readonly MLContext mlContext;
        private ModelSettings modelSettings;
        private OnnxScoringEstimator pipeline;
        /// <summary>
        /// Constructor: model location relative to .exe, mlcontext, model settings of input name and output name
        /// </summary>
        /// <param name="inputModelLocation"></param>
        /// <param name="inputMlContext"></param>
        /// <param name="inputModelSettings"></param>
        public OnnxRNNScorer(string inputModelLocation, MLContext inputMlContext, ModelSettings inputModelSettings) {
            modelLocation = inputModelLocation;
            mlContext = inputMlContext;
            modelSettings = inputModelSettings;
            pipeline = mlContext.Transforms.ApplyOnnxModel(modelFile: modelLocation, outputColumnNames: new[] { modelSettings.modelOutput }, inputColumnNames: new[] { modelSettings.modelInput });
        }

        private ITransformer FitModel(string modelLocation, IDataView data) {
            return pipeline.Fit(data);
        }

        private IEnumerable<float[]> PredictDataUsingModel(IDataView testData, ITransformer model) {
            //Console.WriteLine("=====Identify the objects in the images=====");
            //Console.WriteLine("");
            IDataView scoredData = model.Transform(testData);
            IEnumerable<float[]> probabilities = scoredData.GetColumn<float[]>(modelSettings.modelOutput);
            return probabilities;
        }
        /// <summary>
        /// Evaluate model using data
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public IEnumerable<float[]> Score(IDataView data) {
            var model = FitModel(modelLocation, data);
            return PredictDataUsingModel(data, model);
        }
    }
}
