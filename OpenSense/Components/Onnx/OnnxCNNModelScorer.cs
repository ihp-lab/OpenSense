using Microsoft.ML;
using Microsoft.ML.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenSense.Components.Onnx {
    internal abstract class OnnxCNNModelScorer {
        protected string modelLocation;
        protected MLContext mlContext;
        protected ImageNetSettings imageNetSettings;
        protected ModelSettings modelSettings;
        protected NormalizationSettings normalizationSettings;


        public OnnxCNNModelScorer(string inputModelLocation, MLContext inputMlContext, ImageNetSettings inputImageNetSettings, ModelSettings inputModelSettings, NormalizationSettings inputNormSettings) {
            modelLocation = inputModelLocation;
            mlContext = inputMlContext;
            modelSettings = inputModelSettings;
            imageNetSettings = inputImageNetSettings;
            normalizationSettings = inputNormSettings;
        }

        public OnnxCNNModelScorer() {

        }

        // set up the pipeline for loading model, resizing, normalizing, concatenating.
        protected abstract ITransformer LoadModel(string modelLocation, IDataView data);
        // predict using the loaded model
        protected IEnumerable<float[]> PredictDataUsingModel(IDataView data, ITransformer model) {
            IDataView scoredData = model.Transform(data);
            IEnumerable<float[]> probabilities = scoredData.GetColumn<float[]>(modelSettings.modelOutput);
            return probabilities;
        }
        /// <summary>
        /// pass in data and evaluate the model
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public IEnumerable<float[]> Score(IDataView data) {
            var model = LoadModel(modelLocation, data);
            return PredictDataUsingModel(data, model);
        }

        public void Setup(string inputModelLocation, MLContext inputMlContext, ImageNetSettings inputImageNetSettings, ModelSettings inputModelSettings, NormalizationSettings inputNormSettings)
        {
            modelLocation = inputModelLocation;
            mlContext = inputMlContext;
            modelSettings = inputModelSettings;
            imageNetSettings = inputImageNetSettings;
            normalizationSettings = inputNormSettings;
        }
    }
}
