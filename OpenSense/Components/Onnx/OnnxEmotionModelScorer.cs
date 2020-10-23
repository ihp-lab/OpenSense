using Microsoft.ML;
using Microsoft.ML.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Microsoft.ML.Transforms.Image.ImagePixelExtractingEstimator;

namespace OpenSense.Components.Onnx {
    internal class OnnxEmotionModelScorer : OnnxCNNModelScorer {


        /// <summary>
        /// Construct model scorer for emotion detection
        /// </summary>
        /// <param name="inputModelLocation"></param>
        /// <param name="inputMlContext"></param>
        /// <param name="inputImageNetSettings"></param>
        /// <param name="inputModelSettings"></param>
        public OnnxEmotionModelScorer(string inputModelLocation, MLContext inputMlContext, ImageNetSettings inputImageNetSettings, ModelSettings inputModelSettings, NormalizationSettings inputNormSettings) : base(inputModelLocation, inputMlContext, inputImageNetSettings, inputModelSettings, inputNormSettings) {

        }

        public OnnxEmotionModelScorer() { }


        // set up the pipeline for loading model, resizing, normalizing, concatenating.
        protected override  ITransformer LoadModel(string modelLocation, IDataView data) {

            //var pipeline = mlContext.Transforms.ResizeImages(outputColumnName: "ImageResized", imageWidth: imageNetSettings.imageWidth, imageHeight: imageNetSettings.imageHeight, inputColumnName: "image")
            //    .Append(mlContext.Transforms.ConvertToGrayscale("GrayImage","ImageResized"))
            //    //.Append(mlContext.Transforms.ExtractPixels(modelSettings.modelInput, "GrayImage", ColorBits.All , scaleImage : 1f/255f ))
            //    .Append(mlContext.Transforms.ExtractPixels(outputColumnName: modelSettings.modelInput, inputColumnName: "GrayImage", colorsToExtract: ColorBits.Blue, outputAsFloatArray: true, offsetImage:-255f, scaleImage: 2f / 255f))
            //    .Append(mlContext.Transforms.ApplyOnnxModel(modelFile: modelLocation, outputColumnNames: new[] { modelSettings.modelOutput }, inputColumnNames: new[] { modelSettings.modelInput }));

            var pipeline = mlContext.Transforms.ExtractPixels(outputColumnName: modelSettings.modelInput, inputColumnName: "image", colorsToExtract: ColorBits.Blue, outputAsFloatArray: true, scaleImage: 1f / 255f)
             .Append(mlContext.Transforms.ApplyOnnxModel(modelFile: modelLocation, outputColumnNames: new[] { modelSettings.modelOutput }, inputColumnNames: new[] { modelSettings.modelInput }));


            var model = pipeline.Fit(data);
            return model;

        }
    }
}
