using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using Mediapipe.Net.Framework.Protobuf;
using Microsoft.Psi;
using Microsoft.Psi.Imaging;

namespace OpenSense.Components.PortableFACS {
    public sealed class FaceImageAligner : 
        IConsumer<(IReadOnlyList<NormalizedLandmarkList>, Shared<Image>)>, 
        IProducer<IReadOnlyList<Shared<Image>>> 
        {

        private static readonly IReadOnlyList<int> LeftEyeIndices;
        private static readonly IReadOnlyList<int> RightEyeIndices;
        private static readonly IReadOnlyList<int> MouthOuterEyeIndices;

        public Receiver<(IReadOnlyList<NormalizedLandmarkList>, Shared<Image>)> In { get; }

        public Emitter<IReadOnlyList<Shared<Image>>> Out { get; }

        /// <remarks>
        /// This function replicates the logic in our team's python source code.
        /// </remarks>
        static FaceImageAligner() {
            var mouthOuter = new [] {
                (61, 146), (146, 91), (91, 181), (181, 84), (84, 17),
                (17, 314), (314, 405), (405, 321), (321, 375),
                (375, 291), (61, 185), (185, 40), (40, 39), (39, 37),
                (37, 0), (0, 267),
                (267, 269), (269, 270), (270, 409), (409, 291),
                (78, 95), (95, 88), (88, 178), (178, 87), (87, 14),
                (14, 317), (317, 402), (402, 318), (318, 324),
                (324, 308), (78, 191), (191, 80), (80, 81), (81, 82),
                (82, 13), (13, 312), (312, 311), (311, 310),
                (310, 415), (415, 308),
            };
            var leftEye = new [] { 
                (263, 249), (249, 390), (390, 373), (373, 374),
                (374, 380), (380, 381), (381, 382), (382, 362),
                (263, 466), (466, 388), (388, 387), (387, 386),
                (386, 385), (385, 384), (384, 398), (398, 362),
            };
            var rightEye = new [] {
                (33, 7), (7, 163), (163, 144), (144, 145),
                (145, 153), (153, 154), (154, 155), (155, 133),
                (33, 246), (246, 161), (161, 160), (160, 159),
                (159, 158), (158, 157), (157, 173), (173, 133),
            };
            static IEnumerable<int> process(IEnumerable<(int, int)> data) => 
                data.SelectMany(t => new[] { t.Item1, t.Item2, }).Distinct();

            LeftEyeIndices = process(leftEye).ToArray();
            Debug.Assert(LeftEyeIndices.Count == 16);
            RightEyeIndices = process(rightEye).ToArray();
            Debug.Assert(RightEyeIndices.Count == 16);
            MouthOuterEyeIndices = process(mouthOuter).ToArray();
            Debug.Assert(MouthOuterEyeIndices.Count == 68 - 32);
        }

        public FaceImageAligner(Pipeline pipeline) {
            In = pipeline.CreateReceiver<(IReadOnlyList<NormalizedLandmarkList>, Shared<Image>)>(this, Process, nameof(In));
            Out = pipeline.CreateEmitter<IReadOnlyList<Shared<Image>>>(this, nameof(Out));
        }

        private void Process((IReadOnlyList<NormalizedLandmarkList>, Shared<Image>) data, Envelope envelope) {
            var (faces, image) = data;
            if (faces.Count == 0) {
                Out.Post(Array.Empty<Shared<Image>>(), envelope.OriginatingTime);
                return;
            }
            Debug.Assert(image is not null);
            var width = image.Resource.Width;
            var height = image.Resource.Height;

            /* NOTE:
             * This section replicates the logic in our team's python source code.
             */
            var result = new List<Shared<Image>>();
            foreach (var face in faces.Select(l => l.Landmark)) {
                IEnumerable<Point> process(IReadOnlyList<int> indices) =>
                    indices
                    .Select(i => face[i])
                    .Select(l => (l.X, l.Y))//Extract X and Y, not Z
                    .Select(t => new Point(t.X * width, t.Y * height))//Un-normalize
                    ;
                var leftEyeLMs = process(LeftEyeIndices);
                var rightEyeLMs = process(RightEyeIndices);
                var mouthOuterLms = process(MouthOuterEyeIndices);
                var leftEye = Point.Average(leftEyeLMs);
                var rightEye = Point.Average(rightEyeLMs);
                var eyeAvg = (leftEye + rightEye) * 0.5f;
                var eyeToEye = rightEye - leftEye;
                var mouthAvg = (mouthOuterLms.MinBy(l => l.X) + mouthOuterLms.MaxBy(l => l.X)) * 0.5f;
                var eyeToMouth = mouthAvg - eyeAvg;

            }

            Out.Post(result, envelope.OriginatingTime);
            foreach (var img in result) {
                img.Dispose();
            }
        }
    }
}
