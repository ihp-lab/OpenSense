using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Microsoft.ML;
using Microsoft.ML.Data;
using Microsoft.ML.Transforms.Onnx;
using Microsoft.Psi;
using Microsoft.Psi.Components;
using Microsoft.Psi.Imaging;
using OpenSense.DataStructure;
using OpenSense.Utilities;
using OpenSense.Utilities.DataWriter;
using static Microsoft.ML.Transforms.Image.ImagePixelExtractingEstimator;

namespace OpenSense.Components.Onnx {
    /// <summary>
    /// This detector is reorganized based on Baiyu's implementation
    /// It should be further rewrite totally
    /// </summary>
    public class EmotionDetector : Subpipeline, IProducer<Emotions>, INotifyPropertyChanged {

        private const string MODEL_FILENAME = "emotion_model.onnx";
        private const string EMOTION_MODEL_INPUT_NAME = "input_1";
        private const string EMOTION_MODEL_OUTPUT_NAME = "predictions";
        private const int NET_IMAGE_HEIGHT = 64;
        private const int NET_IMAGE_WIDTH = 64;

        #region INotifyPropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;

        private void SetProperty<T>(ref T field, T value, [CallerMemberName] string propertyName = null) {
            if (!EqualityComparer<T>.Default.Equals(field, value)) {
                field = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            }
        }
        #endregion

        private Connector<Shared<Image>> ImageInConnector;

        public Receiver<Shared<Image>> ImageIn => ImageInConnector.In;

        private Connector<HeadPose> HeadPoseInConnector;

        public Receiver<HeadPose> HeadPoseIn => HeadPoseInConnector.In;

        public Emitter<Emotions> Out { get; private set; }

        private bool mute = false;

        public bool Mute {
            get => mute;
            set => SetProperty(ref mute, value);
        }

        private IDataWriter dataWriter;

        public IDataWriter DataWriter {
            get => dataWriter;
            set => SetProperty(ref dataWriter, value);
        }

        private MLContext mlContext;

        private EstimatorChain<OnnxTransformer> OnnxPipeline;

        public EmotionDetector(Pipeline pipeline) : base(pipeline) {
            ImageInConnector = CreateInputConnectorFrom<Shared<Image>>(pipeline, nameof(ImageIn));
            HeadPoseInConnector = CreateInputConnectorFrom<HeadPose>(pipeline, nameof(HeadPoseIn));
            Out = pipeline.CreateEmitter<Emotions>(this, nameof(Out));
            PipelineCompleted += OnPipelineCompleted;

            var joined = HeadPoseInConnector.Out.Join(ImageInConnector.Out, Reproducible.Exact<Shared<Image>>());
            joined.Do(Process);

            mlContext = new MLContext();
            var modelFilename = OnnxUtility.ModelAbsoluteFilename(MODEL_FILENAME);
            var extractPixels = mlContext.Transforms.ExtractPixels(
                outputColumnName: EMOTION_MODEL_INPUT_NAME, 
                inputColumnName: "image", 
                colorsToExtract: ColorBits.Blue, 
                outputAsFloatArray: true, 
                scaleImage: 1f / 255f
                );
            var emotionModel = mlContext.Transforms.ApplyOnnxModel(
                modelFile: modelFilename, 
                outputColumnNames: new[] { EMOTION_MODEL_OUTPUT_NAME }, 
                inputColumnNames: new[] { EMOTION_MODEL_INPUT_NAME }
                );
            OnnxPipeline = extractPixels.Append(emotionModel);
        }

        private void OnPipelineCompleted(object sender, PipelineCompletedEventArgs e) {
        }

        private void Process(ValueTuple<HeadPose, Shared<Image>> data, Envelope envelope) {
            if (Mute) {
                return;
            }
            try {
                var (headPose, image) = data;
                if (headPose is null || image is null) {
                    return;
                }
                var faceRegion = FaceRegion(headPose);
                using (var bitmap = ImageUtilities.SharedImageToBitmap(image)) {
                    using (var cropped = ImageUtilities.CropBitmap(bitmap, (int)faceRegion.X, (int)faceRegion.Y, (int)faceRegion.Width, (int)faceRegion.Height)) {
                        using (var padded = ImageUtilities.AddPadding(cropped, 40)) {
                            using (var scaled = ImageUtilities.ScaleBitmap(padded, NET_IMAGE_HEIGHT, NET_IMAGE_WIDTH)) {
                                var probabilities = RunModel(scaled);
                                var probList = probabilities.ElementAt(0).ToList();
                                var emotions = new Emotions(probList);
                                DataWriter?.Write(emotions, envelope);
                                Out.Post(emotions, envelope.OriginatingTime);
                            }
                        }
                    }
                }
            } catch (Exception ex) {
                Console.WriteLine(ex.ToString());
                Mute = true;
            }
        }

        private static Rect FaceRegion(HeadPose headPose) {
            var minX = double.PositiveInfinity;
            var maxX = double.NegativeInfinity;
            var minY = double.PositiveInfinity;
            var maxY = double.NegativeInfinity;
            foreach (var mark in headPose.VisiableLandmarks) {
                minX = Math.Min(minX, mark.X);
                maxX = Math.Max(maxX, mark.X);
                minY = Math.Min(minY, mark.Y);
                maxY = Math.Max(maxY, mark.Y);
            }
            var width = Math.Max(0, maxX - minX);
            var height = Math.Max(0, maxY - minY);
            return new Rect(minX, minY, width, height);
        }

        private IEnumerable<float[]> RunModel(System.Drawing.Bitmap input) {
            BitmapObject bitmapObj = new BitmapObject(input);
            IEnumerable<BitmapObject> images = bitmapObj.getBitmapIEnumerable();
            IDataView imageDataView = mlContext.Data.LoadFromEnumerable(images);
            var model = OnnxPipeline.Fit(imageDataView);
            IDataView scoredData = model.Transform(imageDataView);
            IEnumerable<float[]> probabilities = scoredData.GetColumn<float[]>(EMOTION_MODEL_OUTPUT_NAME);
            return probabilities;
        }
    }
}
