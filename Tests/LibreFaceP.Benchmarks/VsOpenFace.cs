using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using BenchmarkDotNet.Attributes;
using OpenFaceInterop;
using Microsoft.Psi.Imaging;
using System.Windows;
using OpenSense.Components.OpenFace;

namespace LibreFace.Benchmarks {
    public class VsOpenFace {

        private CLNF landmarkDetector;
        private FaceDetector faceDetector;
        private FaceAnalyser faceAnalyser;//TODO: Is this required?
        private FaceModelParameters faceModelParameters;

        private int FocalLengthX = 500;
        private int FocalLengthY = 500;

        public VsOpenFace() {
            
        }

        [GlobalSetup]
        public void SetupOpenFace() {
            string rootDirectory = AppDomain.CurrentDomain.BaseDirectory;

            faceModelParameters = new FaceModelParameters(rootDirectory, true, false, false);
            faceModelParameters.optimiseForVideo();

            faceDetector = new FaceDetector(faceModelParameters.GetHaarLocation(), faceModelParameters.GetMTCNNLocation());
            if (!faceDetector.IsMTCNNLoaded()) {
                faceModelParameters.SetFaceDetector(false, true, false);
            }

            landmarkDetector = new CLNF(faceModelParameters);
            faceAnalyser = new FaceAnalyser(rootDirectory, dynamic: true, output_width: 112, mask_aligned: true);

            landmarkDetector.Reset();
            faceAnalyser.Reset();
        }

        public IEnumerable<Image[]> Images() {
            var folder = "C:\\D\\Project\\Other\\LibreFace\\data\\DISFA\\images\\SN001";
            yield return Directory
                .EnumerateFiles(folder)
                .Take(10)
                .Select(Image.FromFile)
                .ToArray();
        }

        [Benchmark]
        [ArgumentsSource(nameof(Images))]
        public List<(Pose, Face)> OpenFace(Image[] images) {
            var result = new List<(Pose, Face)>();
            foreach (var input in images) {
                var width = input.Width;
                var height = input.Height;
                var centerX = width / 2f;
                var centerY = height / 2f;
                static Vector2 pointToVector2(Point p) {
                    return new Vector2((float)p.X, (float)p.Y);
                }
                static Vector2 tupleToVector2(Tuple<float, float> tuple) {
                    return new Vector2(tuple.Item1, tuple.Item2);
                }
                using (var colorSharedImage = ImagePool.GetOrCreate(width, height, input.PixelFormat))
                using (var graySharedImage = ImagePool.GetOrCreate(width, height, PixelFormat.Gray_8bpp)) {
                    input.CopyTo(colorSharedImage.Resource);
                    var colorImageBuffer = new ImageBuffer(width, height, colorSharedImage.Resource.ImageData, colorSharedImage.Resource.Stride);
                    var grayImageBuffer = new ImageBuffer(width, height, graySharedImage.Resource.ImageData, graySharedImage.Resource.Stride);
                    Methods.ToGray(colorImageBuffer, grayImageBuffer);
                    using (var colorRawImage = Methods.ToRaw(colorImageBuffer))
                    using (var grayRawImage = Methods.ToRaw(grayImageBuffer)) {
                        if (landmarkDetector.DetectLandmarksInVideo(colorRawImage, faceModelParameters, grayRawImage)) {

                            var rawAllLandmarks = landmarkDetector.CalculateAllLandmarks();

                            // Pose.
                            var allLandmarks = rawAllLandmarks.Select(tupleToVector2);
                            var visiableLandmarks = landmarkDetector
                                .CalculateVisibleLandmarks()
                                .Select(tupleToVector2);
                            var landmarks3D = landmarkDetector
                                .Calculate3DLandmarks(FocalLengthX, FocalLengthY, centerX, centerY)
                                .Select(m => new Vector3(m.Item1, m.Item2, m.Item3));
                            var poseData = new List<float>();
                            landmarkDetector.GetPose(poseData, FocalLengthX, FocalLengthY, centerX, centerY);
                            var box = landmarkDetector.CalculateBox(FocalLengthX, FocalLengthY, centerX, centerY);
                            var boxConverted = box.Select(line => {
                                var a = pointToVector2(line.Item1);
                                var b = pointToVector2(line.Item2);
                                return (a, b);
                            });
                            var headPose = new Pose(poseData, allLandmarks, visiableLandmarks, landmarks3D, boxConverted);

                            //Face
                            var (actionUnitIntensities, actionUnitOccurences) = faceAnalyser.PredictStaticAUsAndComputeFeatures(colorRawImage, rawAllLandmarks);//image mode, so not using faceAnalyser.AddNextFrame()
                            var actionUnits = actionUnitIntensities.ToDictionary(
                                kv => kv.Key.Substring(2)/*remove prefix "AU"*/.TrimStart('0'),
                                kv => new ActionUnit(intensity: kv.Value, presence: actionUnitOccurences[kv.Key])
                            );
                            var face = new Face(actionUnits);

                            result.Add((headPose, face));
                        }
                    }
                }
            }
            return result;
        }

        [GlobalCleanup]
        public void CleanupOpenFace() {
            faceAnalyser?.Dispose();
            landmarkDetector?.Dispose();
            faceDetector?.Dispose();
            faceModelParameters?.Dispose();
        }
    }
}
