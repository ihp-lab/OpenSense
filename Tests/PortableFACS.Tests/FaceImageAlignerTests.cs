using System.Diagnostics;
using System.Text.Json.Nodes;
using Mediapipe.Net.Framework.Protobuf;
using Microsoft.Psi.Imaging;
using OpenSense.Components.PortableFACS;

namespace PortableFACS.Tests {
    public sealed class FaceImageAlignerTests {
        [Fact]
        public void TestAlign() {
            using var image = Image.FromFile("Resources/SN032/img00001.jpg");
            using var groundTruth = Image.FromFile("Resources/SN032/rst00001.jpg");
            using var stream = new FileStream("Resources/SN032/lmk00001.json", FileMode.Open);
            var facesArr = (JsonArray?)JsonNode.Parse(stream);
            Debug.Assert(facesArr is not null);
            var faces = facesArr.Cast<JsonArray>()
                .Select(lmArr =>
                    lmArr
                    .Cast<JsonArray>()
                    .Select(lm => new NormalizedLandmark { X = (float)lm[0]!, Y = (float)lm[1]!, Z = (float)lm[2]!, })
                    .ToArray()
                )
                .ToArray();

            var (leftEye, rightEye, mouthOuter) = FaceImageAligner.SplitLandmarks(faces.Single(), image.Width, image.Height);
            using var imageRgb = image.Convert(PixelFormat.RGB_24bpp);
            using var result = FaceImageAligner.Align(imageRgb, leftEye, rightEye, mouthOuter);
            using var groundTruthRgb = groundTruth.Convert(PixelFormat.RGB_24bpp);
            var mae = FaceImageAligner.mean_absolute_error(result.Resource, groundTruthRgb);
            Assert.True(mae < 2);
            result.Resource.Save("Resources/SN032/rst00001.jpg");
        }

        [Fact]
        public void TestGaussianFilter() {
            var inputRaw = new int[][] { 
                new []{ 0, 2, 4, 6, 8},
                new []{ 10, 12, 14, 16, 18},
                new []{ 20, 22, 24, 26, 28},
                new []{ 30, 32, 34, 36, 38},
                new []{ 40, 42, 44, 46, 48},
            };
            var input = new TwoDims<Float3>(5, 5);
            for (var i = 0; i < 5; i++) {
                for (var j = 0; j < 5; j++) {
                    var v = (float)inputRaw[i][j];
                    input[i, j] = new Float3(v, v, v);
                }
            }
            var inputPrint = FaceImageAligner.print(input);
            var groundTruthRaw = new double[][] {
                new []{ 5.1244917,  6.4060535,  8.27041  , 10.134766 , 11.416327 },
                new []{ 11.532303 , 12.813865 , 14.678221 , 16.542576 , 17.824139 },
                new []{ 20.854082 , 22.135645 , 24       , 25.864355 , 27.145918 },
                new []{ 30.175861 , 31.457424 , 33.32178  , 35.18614  , 36.4677 },
                new []{ 36.58367  , 37.865234 , 39.72959  , 41.59395  , 42.87551 },
            };
            var groundTruth = new TwoDims<Float3>(5, 5);
            for (var i = 0; i < 5; i++) {
                for (var j = 0; j < 5; j++) {
                    var v = (float)groundTruthRaw[i][j];
                    groundTruth[i, j] = new Float3(v, v, v);
                }
            }
            var groundTruthPrint = FaceImageAligner.print(groundTruth);

            var result = FaceImageAligner.gaussian_filter(input, (1, 1, 0));
            var resultPrint = FaceImageAligner.print(result);
            Assert.True(FaceImageAligner.is_close(result, groundTruth));
        }
    }
}
