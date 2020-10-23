using Microsoft.ML;
using Microsoft.ML.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Microsoft.ML.Transforms.Image.ImagePixelExtractingEstimator;

namespace OpenSense.Components.Onnx {
    internal class OnnxSpeechDetectionModelScorer : OnnxCNNModelScorer {
        private readonly string modelLocation;
        private readonly MLContext mlContext;
        private ImageNetSettings imageNetSettings;
        private ModelSettings modelSettings;
        private NormalizationSettings normalizationSettings;


        public OnnxSpeechDetectionModelScorer(string inputModelLocation, MLContext inputMlContext, ImageNetSettings inputImageNetSettings, ModelSettings inputModelSettings, NormalizationSettings inputNormSettings) : base(inputModelLocation, inputMlContext, inputImageNetSettings, inputModelSettings, inputNormSettings) {
            
        }

        public OnnxSpeechDetectionModelScorer() { }

        // set up the pipeline for loading model, resizing, normalizing, concatenating.
        protected override ITransformer LoadModel(string modelLocation, IDataView data) {
            //Console.WriteLine("Read model");
            //Console.WriteLine($"Model location: {modelLocation}");
            var pipeline = mlContext.Transforms.ResizeImages(outputColumnName: "ImageResized", imageWidth: imageNetSettings.imageWidth, imageHeight: imageNetSettings.imageHeight, inputColumnName: "image")
                    .Append(mlContext.Transforms.ExtractPixels("Red", "ImageResized",
                            colorsToExtract: ColorBits.Red, offsetImage: normalizationSettings.redMean * 255, scaleImage: 1 / (normalizationSettings.redStd * 255)))
                    .Append(mlContext.Transforms.ExtractPixels("Green", "ImageResized",
                            colorsToExtract: ColorBits.Green, offsetImage: normalizationSettings.greenMean * 255, scaleImage: 1 / (normalizationSettings.greenStd * 255)))
                    .Append(mlContext.Transforms.ExtractPixels("Blue", "ImageResized",
                            colorsToExtract: ColorBits.Blue, offsetImage: normalizationSettings.blueMean * 255, scaleImage: 1 / (normalizationSettings.blueStd * 255)))
                    .Append(mlContext.Transforms.Concatenate(modelSettings.modelInput, new[] { "Red", "Green", "Blue" }))
                .Append(mlContext.Transforms.ApplyOnnxModel(modelFile: modelLocation, outputColumnNames: new[] { modelSettings.modelOutput }, inputColumnNames: new[] { modelSettings.modelInput }));

            var model = pipeline.Fit(data);
            return model;

        }

    }
}
