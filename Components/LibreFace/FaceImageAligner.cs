﻿using System;
using System.Buffers;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Text;
using Mediapipe.Net.Framework.Protobuf;
using Microsoft.Psi;
using Microsoft.Psi.Imaging;
#if OPENCV
using OpenCvSharp;
#endif

namespace OpenSense.Components.LibreFace {
    public sealed class FaceImageAligner :
        IConsumer<(IReadOnlyList<NormalizedLandmarkList>, Shared<Image>)>,
        IProducer<IReadOnlyList<Shared<Image>>> {

        private const int AlignOutputSize = 256;
        private const int CropOutputSize = 224;

        private static readonly IReadOnlyList<int> LeftEyeIndices;
        private static readonly IReadOnlyList<int> RightEyeIndices;
        private static readonly IReadOnlyList<int> MouthOuterIndices;

        public Receiver<(IReadOnlyList<NormalizedLandmarkList>, Shared<Image>)> In { get; }

        public Emitter<IReadOnlyList<Shared<Image>>> Out { get; }

        /// <remarks>
        /// This function replicates the logic in LibreFace's python source code.
        /// </remarks>
        static FaceImageAligner() {
            var mouthOuter = new[] {
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
            var leftEye = new[] {
                (263, 249), (249, 390), (390, 373), (373, 374),
                (374, 380), (380, 381), (381, 382), (382, 362),
                (263, 466), (466, 388), (388, 387), (387, 386),
                (386, 385), (385, 384), (384, 398), (398, 362),
            };
            var rightEye = new[] {
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
            MouthOuterIndices = process(mouthOuter).ToArray();
            Debug.Assert(MouthOuterIndices.Count == 72 - 32);
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

            var result = Run(image.Resource, faces);

            Out.Post(result, envelope.OriginatingTime);
            foreach (var img in result) {
                img.Dispose();
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static IReadOnlyList<Shared<Image>> Run(Image image, IReadOnlyList<NormalizedLandmarkList> faces) {
            var result = new List<Shared<Image>>();
            foreach (var face in faces.Select(l => l.Landmark)) {
                var (leftEye, rightEye, mouthOuter) = SplitLandmarks(face, image.Width, image.Height);
                using var aligned = Align(image, leftEye, rightEye, mouthOuter);
                var img = CenterCrop(aligned.Resource, CropOutputSize);
                result.Add(img);
            }
            return result;
        }

        internal static (IEnumerable<Float2> LeftEyeLMs, IEnumerable<Float2> RightEyeLMs, IEnumerable<Float2> MouthOuterLMs) SplitLandmarks(IReadOnlyList<NormalizedLandmark> faceLandmarks, int width, int height) {
            IEnumerable<Float2> process(IReadOnlyList<int> indices) =>
                indices
                .Where(i => i < faceLandmarks.Count)
                .Select(i => faceLandmarks[i])
                .Select(lm => (Float2)lm)//Extract X and Y, not Z
                .Select(lm => lm * (width, height))//Un-normalize
                ;
            return (process(LeftEyeIndices), process(RightEyeIndices), process(MouthOuterIndices));
        }

        /// <summary>
        /// This function replicates the logic in LibreFace's python source code.
        /// </summary>
        internal static Shared<Image> Align(in Image image, IEnumerable<Float2> leftEyeLMs, IEnumerable<Float2> rightEyeLMs, IEnumerable<Float2> mouthOuterLMs) {
            (leftEyeLMs, rightEyeLMs) = (rightEyeLMs, leftEyeLMs);//Note: The python code's naming is wrong, here we follow the wrong naming.
            Debug.Assert(image is not null);
            Debug.Assert(image.PixelFormat == PixelFormat.RGB_24bpp);//Must be RGB format, BGR is not acceptable and will produce wrong result.
            var width = image.Width;
            var height = image.Height;
            var temp_image_01 = ImagePool.GetOrCreate(width, height, image.PixelFormat);
            var leftEye = np_mean(leftEyeLMs, axis: 0);
            var rightEye = np_mean(rightEyeLMs, axis: 0);
            var eyeAvg = (leftEye + rightEye) * 0.5f;
            var eyeToEye = rightEye - leftEye;
            var mouthAvg = (mouthOuterLMs.MinBy(l => l.I0) + mouthOuterLMs.MaxBy(l => l.I0)) * 0.5f;
            var eyeToMouth = mouthAvg - eyeAvg;
            var float2_01 = np_flipud(eyeToMouth);
            var float2_02 = float2_01 * (-1, 1);
            var x1 = eyeToEye - float2_02;
            var float_01 = np_hypot(x1);
            var x2 = x1 / float_01;
            var temp6 = op_max(np_hypot(eyeToEye) * 2.0f, np_hypot(eyeToMouth) * 1.8f);
            var x3 = x2 * temp6;
            const float xScale = 1f;
            var x4 = x3 * xScale;
            const float yScale = 1f;
            var temp9 = np_flipud(x4);
            var y = temp9 * (-yScale, yScale);
            const float emScale = 0.1f;
            var c = eyeAvg + eyeToMouth * emScale;
            var quad = new Quad(
                c - x4 - y,
                c - x4 + y,
                c + x4 + y,
                c + x4 - y
            );
            var qSize = np_hypot(x4) * 2;
            const int outputSize = AlignOutputSize;
            var temp13 = qSize / outputSize * 0.5f;
            var temp14 = np_floor(temp13);
            var shrink = (int)temp14;
            image.CopyTo(temp_image_01.Resource);
            if (shrink > 1) {
                var temp16 = width / (float)shrink;
                var temp17 = (int)np_rint(temp16);
                var temp18 = height / (float)shrink;
                var temp19 = (int)np_rint(temp18);
                var rSize = (Width: temp17, Height: temp19);
                PIL_resize_(ref temp_image_01, rSize);
                np_op_div_(ref quad, shrink);
                qSize = qSize / shrink;
            }
            var temp21 = (int)np_rint(qSize * 0.1f);
            var border = op_max(temp21, 3);
            var temp22 = (int)np_floor(op_min(quad[":,0"]));
            var temp23 = (int)np_floor(op_min(quad[":,1"]));
            var temp24 = (int)np_ceil(op_max(quad[":,0"]));
            var temp25 = (int)np_ceil(op_max(quad[":,1"]));
            var crop1 = new Int4(I0: temp22, I1: temp23, I2: temp24, I3: temp25);
            var temp27 = op_max(crop1.I0 - border, 0);
            var temp28 = op_max(crop1.I1 - border, 0);
            var temp29 = op_min(crop1.I2 + border, width);
            var temp30 = op_min(crop1.I3 + border, height);
            var crop2 = new Int4(temp27, temp28, temp29, temp30);
            var temp32 = crop2.I2 - crop2.I0;
            var temp33 = crop2.I3 - crop2.I1;
            if (temp32 < width || temp33 < height) {
                PIL_crop_(ref temp_image_01, crop2);
                np_op_minus_(ref quad, np_op_slice(crop2, 0, 2));
            }
            var temp35 = (int)np_floor(op_min(quad[":,0"]));
            var temp36 = (int)np_floor(op_min(quad[":,1"]));
            var temp37 = (int)np_ceil(op_max(quad[":,0"]));
            var temp38 = (int)np_ceil(op_max(quad[":,1"]));
            var pad1 = new Int4(temp35, temp36, temp37, temp38);
            var temp40 = op_max(-pad1.I0 + border, 0);
            var temp41 = op_max(-pad1.I1 + border, 0);
            var temp42 = op_max(pad1.I2 - width + border, 0);
            var temp43 = op_max(pad1.I3 - height + border, 0);
            var pad2 = new Int4(temp40, temp41, temp42, temp43);
            const bool enablePadding = true;
            if (enablePadding && op_max(pad2) > border - 4) {
                var temp45 = (int)np_rint(qSize * 0.3f);
                var pad3 = np_maximum(pad2, temp45);
                np_pad_(ref temp_image_01, ((pad3.I1, pad3.I3), (pad3.I0, pad3.I2)));
                var h = temp_image_01.Resource.Height;
                var w = temp_image_01.Resource.Width;
                var xOGrid = Enumerable.Range(0, w).Select(i => (float)i);
                var yOGrid = Enumerable.Range(0, h).Select(i => (float)i);
                var temp48 = xOGrid.Select(v => v / pad3.I0);
                var temp49 = xOGrid.Select(v => (w - 1 - v) / pad3.I2);
                var temp50 = np_minimum(temp48, temp49)
                    .Select(v => 1f - v);
                var temp51 = yOGrid.Select(v => v / pad3.I1);
                var temp52 = yOGrid.Select(v => (h - 1 - v) / pad3.I3);
                var temp53 = np_minimum(temp51, temp52)
                    .Select(v => 1f - v);
                var temp_float_h_w_01 = new TwoDims<float>(rows: h, columns: w);
                np_maximum2D(
                    cols: temp50.ToArray(),
                    rows: temp53.ToArray(),
                    output: ref temp_float_h_w_01//mask
                );
                var blur = qSize * 0.02f;
                var temp_float_3_h_w_01 = new TwoDims<Vector3>(rows: h, columns: w);
                np_float32(
                    img: temp_image_01,//result
                    ref temp_float_3_h_w_01//imgF
                );
                var temp_float_3_h_w_02 = new TwoDims<Vector3>(rows: h, columns: w);
                var temp_float_3_h_w_03 = new TwoDims<Vector3>(rows: h, columns: w);
                gaussian_filter(
                    img: ref temp_float_3_h_w_01,
                    sigmas: (blur, blur, 0),
                    interim: ref temp_float_3_h_w_02,
                    output: ref temp_float_3_h_w_03//temp55
                );
                np_op_minus(
                    left: ref temp_float_3_h_w_03,//temp55
                    right: ref temp_float_3_h_w_01,//imgF
                    output: ref temp_float_3_h_w_02//temp56
                );
                var temp_float_h_w_02 = new TwoDims<float>(rows: h, columns: w);
                np_op_mult(
                    left: ref temp_float_h_w_01,//mask
                    right: 3,
                    output: ref temp_float_h_w_02//temp57
                );
                var temp_float_h_w_03 = new TwoDims<float>(rows: h, columns: w);
                np_op_add(//TODO: why inplace op produce wrong result?
                    left: ref temp_float_h_w_02,//temp57
                    right: 1,
                    output: ref temp_float_h_w_03//temp58
                );
                np_clip__(
                    value: ref temp_float_h_w_03,//temp58 -> temp59
                    min: 0,
                    max: 1
                );
                np_op_mult__left(
                    left: ref temp_float_3_h_w_02,//temp56 -> temp60
                    right: ref temp_float_h_w_03//temp59
                );
                np_op_add__left(
                    left: ref temp_float_3_h_w_01,//imgF
                    right: ref temp_float_3_h_w_02//temp60
                );
                var temp62 = np_median(temp_float_3_h_w_01, (0, 1));
                np_op_minus(
                    left: temp62,
                    right: ref temp_float_3_h_w_01,//imgF
                    output: ref temp_float_3_h_w_02//temp63
                );
                np_clip__(
                    value: ref temp_float_h_w_01,//mask -> temp64
                    min: 0,
                    max: 1
                );
                np_op_mult__left(
                    left: ref temp_float_3_h_w_02,//temp63 -> temp65
                    right: ref temp_float_h_w_01//temp64
                );
                np_op_add__left(
                    left: ref temp_float_3_h_w_01,//imgF -> imgF
                    right: ref temp_float_3_h_w_02//temp65
                );
                np_rint__(
                    value: ref temp_float_3_h_w_01 //imgF -> temp67
                );
                np_clip__(
                    value: ref temp_float_3_h_w_01,//temp67 -> temp68
                    min: 0,
                    max: 255
                );
                np_uint8(
                    value: ref temp_float_3_h_w_01, //temp68
                    output: temp_image_01//result
                );
                const bool alpha = false;
                if (alpha) {
                    throw new NotImplementedException();
                } else {
                    //nothing
                }
                var temp70 = np_op_slice(pad3, 0, 2);
                np_op_add_(ref quad, temp70);

                temp_float_h_w_03.Dispose();
                temp_float_h_w_02.Dispose();
                temp_float_3_h_w_03.Dispose();
                temp_float_3_h_w_02.Dispose();
                temp_float_3_h_w_01.Dispose();
                temp_float_h_w_01.Dispose();
            }
            const int transformSize = outputSize;//was 4096, and it is too large
            var temp72 = (transformSize, transformSize);
            var temp73 = np_op_add(quad, 0.5f);
            PIL_transform_Qud_Bilinear_(ref temp_image_01, temp72, temp73);
            var temp75 = (outputSize, outputSize);
            PIL_resize_(ref temp_image_01, temp75);
            return temp_image_01;
        }

        internal static Shared<Image> CenterCrop(in Image image, int size) {
            Debug.Assert(image.Width == AlignOutputSize);
            Debug.Assert(image.Width == image.Height);
            Debug.Assert(image.Width >= size);
            var result = ImagePool.GetOrCreate(size, size, image.PixelFormat);
            int offset = (image.Width - size) / 2;
            image.Crop(result.Resource, offset, offset, size, size);
            return result;
        }

        #region Functions
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static float op_min(Float4 value) =>
            MathF.Min(MathF.Min(value.I0, value.I1), MathF.Min(value.I2, value.I3));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static int op_min(int left, int right) =>
        Math.Min(left, right);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static float op_max(Float4 value) =>
            MathF.Max(MathF.Max(value.I0, value.I1), MathF.Max(value.I2, value.I3));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static float op_max(float left, float right) =>
            MathF.Max(left, right);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static int op_max(int left, int right) =>
            Math.Max(left, right);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static int op_max(Int4 value) =>
            Math.Max(Math.Max(value.I0, value.I1), Math.Max(value.I2, value.I3));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static Float2 np_mean(IEnumerable<Float2> value, int axis) {
            Debug.Assert(axis == 0);
            var count = 0;
            var x = 0d;
            var y = 0d;
            foreach (var point in value) {
                x += point.I0;
                y += point.I1;
                count++;
            }
            var result = new Float2((float)(x / count), (float)(y / count));
            return result;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static Float2 np_flipud(Float2 value) =>
            new Float2(value.I1, value.I0);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static float np_hypot(Float2 value) =>
            MathF.Sqrt(value.I0 * value.I0 + value.I1 * value.I1);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static float np_floor(float value) =>
            MathF.Floor(value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static float np_ceil(float value) =>
            MathF.Ceiling(value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static float np_rint(float value) =>
            MathF.Round(value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static Vector3 np_rint(Vector3 value) =>
            new Vector3(np_rint(value.X), np_rint(value.Y), np_rint(value.Z));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void np_rint__(ref TwoDims<Vector3> value) {
            for (var i = 0; i < value.Rows; i++) {
                for (var j = 0; j < value.Columns; j++) {
                    value[i, j] = np_rint(value[i, j]);
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void np_clip__(ref TwoDims<float> value, float min, float max) {
            for (var i = 0; i < value.Rows; i++) {
                for (var j = 0; j < value.Columns; j++) {
                    value[i, j] = MathF.Max(min, MathF.Min(max, value[i, j]));
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static float np_clip(float value, float min, float max) =>
            MathF.Max(min, MathF.Min(max, value));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static Vector3 np_clip(Vector3 value, float min, float max) =>
            np_clip(value, new(min, min, min), (Vector3)new(max, max, max));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static Vector3 np_clip(Vector3 value, Vector3 min, Vector3 max) =>
            Vector3.Clamp(value, min, max);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void np_clip__(ref TwoDims<Vector3> value, float min, float max) {
            var vMin = new Vector3(min, min, min);
            var vMax = new Vector3(max, max, max);
            for (var i = 0; i < value.Rows; i++) {
                for (var j = 0; j < value.Columns; j++) {
                    value[i, j] = np_clip(value[i, j], vMin, vMax);
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void np_op_mult(ref TwoDims<float> left, float right, ref TwoDims<float> output) {
            Debug.Assert(output.Rows == left.Rows && output.Columns == left.Columns);
            for (var i = 0; i < left.Rows; i++) {
                for (var j = 0; j < left.Columns; j++) {
                    output[i, j] = left[i, j] * right;
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void np_op_mult__left(ref TwoDims<Vector3> left, ref TwoDims<float> right) {
            Debug.Assert(left.Rows == right.Rows);
            Debug.Assert(left.Columns == right.Columns);

            for (var i = 0; i < left.Rows; i++) {
                for (var j = 0; j < left.Columns; j++) {
                    left[i, j] = left[i, j] * right[i, j];
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void np_op_div_(ref Quad left, float right) {
            for (var i = 0; i < Quad.Count; i++) {
                left[i] = left[i] / right;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void np_op_minus_(ref Quad left, Float2 right) {
            for (var i = 0; i < Quad.Count; i++) {
                left[i] = left[i] - right;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void np_op_minus(ref TwoDims<Vector3> left, ref TwoDims<Vector3> right, ref TwoDims<Vector3> output) {
            Debug.Assert(left.Rows == right.Rows);
            Debug.Assert(left.Columns == right.Columns);
            Debug.Assert(output.Rows == left.Rows && output.Columns == left.Columns);

            for (var i = 0; i < left.Rows; i++) {
                for (var j = 0; j < left.Columns; j++) {
                    output[i, j] = left[i, j] - right[i, j];
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void np_op_minus(Vector3 left, ref TwoDims<Vector3> right, ref TwoDims<Vector3> output) {
            Debug.Assert(output.Rows == right.Rows && output.Columns == right.Columns);
            for (var i = 0; i < right.Rows; i++) {
                for (var j = 0; j < right.Columns; j++) {
                    output[i, j] = left - right[i, j];
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void np_op_add(ref TwoDims<float> left, float right, ref TwoDims<float> output) {
            Debug.Assert(output.Rows == left.Rows && output.Columns == left.Columns);
            for (var i = 0; i < left.Rows; i++) {
                for (var j = 0; j < left.Columns; j++) {
                    output[i, j] = left[i, j] + right;
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static Quad np_op_add(Quad left, float right) {
            var result = new Quad();
            for (var i = 0; i < Quad.Count; i++) {
                result[i] = left[i] + right;
            }
            return result;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void np_op_add__left(ref TwoDims<Vector3> left, ref TwoDims<Vector3> right) {
            Debug.Assert(left.Rows == right.Rows);
            Debug.Assert(left.Columns == right.Columns);
            for (var i = 0; i < left.Rows; i++) {
                for (var j = 0; j < left.Columns; j++) {
                    left[i, j] += right[i, j];
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void np_op_add_(ref Quad left, Float2 right) {
            for (var i = 0; i < Quad.Count; i++) {
                left[i] += right;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static Float2 np_op_slice(Int4 left, int start, int end) {
            Debug.Assert(start == 0);
            Debug.Assert(end == 2);
            return new Float2(left.I0, left.I1);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static Int4 np_maximum(Int4 left, int right) =>
            (Math.Max(left.I0, right), Math.Max(left.I1, right), Math.Max(left.I2, right), Math.Max(left.I3, right));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static IEnumerable<float> np_minimum(IEnumerable<float> left, IEnumerable<float> right) {
            Debug.Assert(left.Count() == right.Count());
            return left.Zip(right).Select(t => MathF.Min(t.First, t.Second));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void np_maximum2D(IReadOnlyList<float> rows, IReadOnlyList<float> cols, ref TwoDims<float> output) {
            Debug.Assert(output.Rows == rows.Count && output.Columns == cols.Count);
            for (var i = 0; i < rows.Count; i++) {
                for (var j = 0; j < cols.Count; j++) {
                    output[i, j] = MathF.Max(rows[i], cols[j]);
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static unsafe void np_float32(Shared<Image> img, ref TwoDims<Vector3> output) {
            Debug.Assert(img.Resource.PixelFormat == PixelFormat.RGB_24bpp);
            Debug.Assert(output.Rows == img.Resource.Height && output.Columns == img.Resource.Width);
            var span = new ReadOnlySpan<byte>(img.Resource.ImageData.ToPointer(), img.Resource.Size);
            var stride = img.Resource.Stride;
            for (var i = 0; i < img.Resource.Height; i++) {
                for (var j = 0; j < img.Resource.Width; j++) {
                    output[i, j] = get_pixel(span, stride, j, i);
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static unsafe void np_uint8(ref TwoDims<Vector3> value, Shared<Image> output) {
            Debug.Assert(value.Rows == output.Resource.Height);
            Debug.Assert(value.Columns == output.Resource.Width);
            Debug.Assert(output.Resource.PixelFormat == PixelFormat.RGB_24bpp);
            var span = new Span<byte>(output.Resource.ImageData.ToPointer(), output.Resource.Size);
            var stride = output.Resource.Stride;

            for (var i = 0; i < value.Rows; i++) {
                for (var j = 0; j < value.Columns; j++) {
                    var v = value[i, j];
                    Debug.Assert(0 <= v.X && v.X <= byte.MaxValue);
                    Debug.Assert(0 <= v.Y && v.Y <= byte.MaxValue);
                    Debug.Assert(0 <= v.Z && v.Z <= byte.MaxValue);
                    set_pixel(span, stride, j, i, v);
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static Vector3 np_median(TwoDims<Vector3> value, (int, int) axis) {
            Debug.Assert(axis == (0, 1));
            var result = new float[3];
            for (var k = 0; k < 3; k++) {
                var channelValues = Enumerable
                    .Range(0, value.Rows)
                    .SelectMany(i =>
                        Enumerable
                            .Range(0, value.Columns)
                            .Select(j => (i, j))
                        )
                    .Select(t => value[t.i, t.j][k])
                    .Order()
                    .ToArray();//TODO: optimize
                if (channelValues.Length % 2 != 0) {//odd
                    result[k] = channelValues[channelValues.Length / 2];
                } else {//even
                    result[k] = (channelValues[channelValues.Length / 2 - 1] + channelValues[channelValues.Length / 2]) / 2;
                }
            }
            return new(result[0], result[1], result[2]);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void PIL_resize_(ref Shared<Image> img, (int, int) sizes) {
            if (sizes == (img.Resource.Width, img.Resource.Height)) {
                return;
            }
            var temp = ImagePool.GetOrCreate(sizes.Item1, sizes.Item2, img.Resource.PixelFormat);
            img.Resource.Resize(temp.Resource, sizes.Item1, sizes.Item2, SamplingMode.Bicubic);
            img.Dispose();
            img = temp;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void PIL_crop_(ref Shared<Image> img, Int4 edges) {
            var (left, upper, right, lower) = edges;
            var width = right - left;
            var height = lower - upper;
            var temp = ImagePool.GetOrCreate(width, height, img.Resource.PixelFormat);
            img.Resource.Crop(temp.Resource, left, upper, width, height, clip: false);//TODO: What is clip for?
            img.Dispose();
            img = temp;
        }

        /// <remarks>
        /// https://github.com/python-pillow/Pillow/blob/d1f4917eb41abbb6e3e6ca822050cbab12ee1dfe/src/PIL/Image.py#L2683
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void PIL_transform_Qud_Bilinear_(ref Shared<Image> img, (int, int) outputSize, Quad quad) {
            Debug.Assert(outputSize.Item1 == outputSize.Item2);
            var (w, h) = outputSize;
            var temp = ImagePool.GetOrCreate(w, h, img.Resource.PixelFormat);
#if OPENCV
            unsafe {
                using var src = new Mat(img.Resource.Height, img.Resource.Width, MatType.CV_8UC3, img.Resource.ImageData, img.Resource.Stride);
                using var dst = new Mat(h, w, MatType.CV_8UC3, temp.Resource.ImageData, temp.Resource.Stride);
                static Point2f cvt(Float2 p) => new Point2f(p.I0, p.I1);
                var srcQuad = new Point2f[] {
                    cvt(quad.I0),
                    cvt(quad.I1),
                    cvt(quad.I2),
                    cvt(quad.I3),
                };
                var dstQuad = new Point2f[] {
                    new Point2f(0, 0),
                    new Point2f(0, dst.Rows - 1),
                    new Point2f(dst.Cols - 1, dst.Rows - 1),
                    new Point2f(dst.Cols - 1, 0),
                };
                using var perspectiveMatrix = Cv2.GetPerspectiveTransform(srcQuad, dstQuad);
                Cv2.WarpPerspective(src, dst, perspectiveMatrix, new Size(w, h), InterpolationFlags.Linear, BorderTypes.Reflect101);
            }
#else
            var (nw, sw, se, ne) = quad;
            var (x0, y0) = nw;
            var As = 1f / w;
            var At = 1f / h;
            var data = (
                x0,
                (ne.I0 - x0) * As,
                (sw.I0 - x0) * At,
                (se.I0 - sw.I0 - ne.I0 + x0) * As * At,
                y0,
                (ne.I1 - y0) * As,
                (sw.I1 - y0) * At,
                (se.I1 - sw.I1 - ne.I1 + y0) * As * At
            );
            
            ImagingGenericTransform_quad_transform(temp.Resource, img.Resource, data);
#endif
            img.Dispose();
            img = temp;
        }

        /// <remarks>
        /// https://github.com/python-pillow/Pillow/blob/95cff6e959bb3c37848158ed2145d49d49806a31/src/libImaging/Geometry.c#L420
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static (float x, float y) quad_transform(int x, int y, in (float, float, float, float, float, float, float, float) data) {//Note: the original code uses float64
            var (a0, a1, a2, a3, a4, a5, a6, a7) = data;
            float xin = x + 0.5f;
            float yin = y + 0.5f;

            var xout = a0 + a1 * xin + a2 * yin + a3 * xin * yin;
            var yout = a4 + a5 * xin + a6 * yin + a7 * xin * yin;
            return (xout, yout);
        }

        ///<remarks>
        ///https://github.com/python-pillow/Pillow/blob/95cff6e959bb3c37848158ed2145d49d49806a31/src/libImaging/Geometry.c#L774
        ///</remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static unsafe void ImagingGenericTransform_quad_transform(
            in Image imgOut,
            in Image imgIn,
            (float, float, float, float, float, float, float, float) data
        ) {
            Debug.Assert(imgIn.PixelFormat == PixelFormat.RGB_24bpp);
            Debug.Assert(imgOut.PixelFormat == PixelFormat.RGB_24bpp);
            var spanIn = new ReadOnlySpan<byte>(imgIn.ImageData.ToPointer(), imgIn.Size);
            var spanOut = new Span<byte>(imgOut.ImageData.ToPointer(), imgOut.Size);
            var strideIn = imgIn.Stride;
            var strideOut = imgOut.Stride;
            var widthIn = imgIn.Width;
            var widthOut = imgOut.Width;
            var heightIn = imgIn.Height;
            var heightOut = imgOut.Height;
            const int fill = 1;
            const int x0 = 0;
            const int y0 = 0;
            var x1 = imgOut.Width;
            var y1 = imgOut.Height;
            var min = Vector3.Zero;
            var max = new Vector3(byte.MaxValue, byte.MaxValue, byte.MaxValue);
            for (var y = y0; y < y1; y++) {
                for (var x = x0; x < x1; x++) {
                    var (xx, yy) = quad_transform(x - x0, y - y0, data);
                    var pixel = bilinear_interpolation(spanIn, strideIn, widthIn, heightIn, yy, xx);
                    pixel = np_clip(np_rint(pixel), min, max);
                    set_pixel(spanOut, strideOut, x, y, pixel);
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void np_pad_(ref Shared<Image> img, ((int, int), (int, int)) values) {
            var ((heightPadBefore, heightPadAfter), (widthPadBefore, widthPadAfter)) = values;
            var width = img.Resource.Width + widthPadBefore + widthPadAfter;
            var height = img.Resource.Height + heightPadBefore + heightPadAfter;
            var temp = ImagePool.GetOrCreate(width, height, img.Resource.PixelFormat);
            temp.Resource.FillRectangle(new(0, 0, width, height), System.Drawing.Color.Black);
            img.Resource.CopyTo(new(0, 0, img.Resource.Width, img.Resource.Height), temp.Resource, new(widthPadBefore, heightPadBefore));
            img.Dispose();
            img = temp;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static void gaussian_filter(ref TwoDims<Vector3> img, Float3 sigmas, ref TwoDims<Vector3> interim, ref TwoDims<Vector3> output) {
            Debug.Assert(sigmas.I2 == 0);//Do not apply to color channels
            Debug.Assert(sigmas.I0 == sigmas.I1);
            Debug.Assert(interim.Rows == img.Rows && interim.Columns == img.Columns);
            Debug.Assert(output.Rows == img.Rows && output.Columns == img.Columns);

            var sd = sigmas.I0;
            const float truncate = 4;
            var lw = (int)(truncate * sd + 0.5f);
            const int order = 0;
            var weights = gaussian_kernel_1d(sd, order, lw);
            //Array.Reverse(weights);

            correlate1d(img: ref img, weights: weights, axis: 0, output: ref interim);
            correlate1d(img: ref interim, weights: weights, axis: 1, output: ref output);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void correlate1d(ref TwoDims<Vector3> img, float[] weights, int axis, ref TwoDims<Vector3> output) {//Note: the original code uses float64, and we do see precision loss here
            Debug.Assert(output.Rows == img.Rows && output.Columns == img.Columns);
            Debug.Assert(weights.Length % 2 == 1);
            var half = weights.Length / 2;
            switch (axis) {
                case 0:
                    for (var j = 0; j < img.Columns; j++) {
                        for (var i = 0; i < img.Rows; i++) {
                            var sum = new Vector3();
                            for (var k = 0; k < weights.Length; k++) {
                                var ii = extension_mode_reflect(i - half + k, img.Rows);
                                var jj = extension_mode_reflect(j, img.Columns);
                                var val = img[ii, jj];
                                var add = val * weights[k];
                                sum += add;
                            }
                            output[i, j] = sum;
                        }
                    }
                    break;
                case 1:
                    for (var i = 0; i < img.Rows; i++) {
                        for (var j = 0; j < img.Columns; j++) {
                            var sum = new Vector3();
                            for (var k = 0; k < weights.Length; k++) {
                                var ii = extension_mode_reflect(i, img.Rows);
                                var jj = extension_mode_reflect(j - half + k, img.Columns);
                                var val = img[ii, jj];
                                var add = val * weights[k];
                                sum += add;
                            }
                            output[i, j] = sum;
                        }
                    }
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(axis));
            }
        }

        /// <remarks>
        /// https://github.com/scipy/scipy/blob/c1ed5ece8ffbf05356a22a8106affcd11bd3aee0/scipy/ndimage/_filters.py#L180
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static float[] gaussian_kernel_1d(float sigma, int order, int radius) {//Note: the original code uses float64, and we do see precision loss here
            Debug.Assert(order == 0);
            var sigma2 = sigma * sigma;
            var x = Enumerable.Range(-radius, (radius + 1) - -radius);
            var phi_x = x.Select(xx => MathF.Exp(-0.5f / sigma2 * MathF.Pow(xx, 2)))
                .ToArray();
            phi_x = phi_x.Select(xx => xx / phi_x.Sum())
                .ToArray();
            return phi_x;
        }

         [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static int extension_mode_reflect(int x, int size) {
            if (x < 0) {
                x = x <= -1 ? -x - 1 : 0;
            }
            if (x >= size) {
                x = x >= size ? (size - 1) - (x - size) : size - 1;
            }
            return x;
        }


         [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static (float y, float x) extension_mode_mirror(float y, float x, int rows, int columns) {
            bool updated;
            do {
                updated = false;
                if (y < 0) {
                    y = -y;
                    updated = true;
                }
                if (x < 0) {
                    x = -x;
                    updated = true;
                }
                if (y >= rows) {
                    y = (rows - 1) - (y - (rows - 1));
                    updated = true;
                }
                if (x >= columns) {
                    x = (columns - 1) - (x - (columns - 1));
                    updated = true;
                }
            } while (updated);
            return (y, x);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static Vector3 linear_interpolation(Vector3 v1, float w1, Vector3 v2, float w2) => v1 * w1 + v2 * w2;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static Vector3 bilinear_interpolation(ReadOnlySpan<byte> span, int stride, int width, int height, float y, float x) {
            /* mirror (not reflect) */
            var x1 = (int)MathF.Floor(x);
            var x2 = (int)MathF.Ceiling(x);
            var y1 = (int)MathF.Floor(y);
            var y2 = (int)MathF.Ceiling(y);
            x1 = extension_mode_reflect(x1, width);
            x2 = extension_mode_reflect(x2, width);
            y1 = extension_mode_reflect(y1, height);
            y2 = extension_mode_reflect(y2, height);

            Vector3 result;
            /*
            var needX = x1 != x2;
            var needY = y1 != y2;
            switch (needX, needY) {
                case (false, false):
                    result = get_pixel(span, stride, x1, y1);
                    break;
                case (true, false):
                    result = linear_interpolation(get_pixel(span, stride, x1, y1), x2 - x, get_pixel(span, stride, x2, y1), x - x1);
                    break;
                case (false, true):
                    result = linear_interpolation(get_pixel(span, stride, x1, y1), y2 - y, get_pixel(span, stride, x1, y2), y - y1);
                    break;
                case (true, true):
                    var r1 = linear_interpolation(get_pixel(span, stride, x1, y1), x2 - x, get_pixel(span, stride, x2, y1), x - x1);
                    var r2 = linear_interpolation(get_pixel(span, stride, x1, y2), x2 - x, get_pixel(span, stride, x2, y2), x - x1);
                    result = linear_interpolation(r1, y2 - y, r2, y - y1);
                    break;
            }
            */
            var offset = y1 * stride + x1 * 3;
            var sx = (x2 - x1) * 3;
            var sy = (y2 - y1) * stride;
            var f = x - x1;
            var r1 = Vector3.Lerp(get_pixel(span, offset), get_pixel(span, offset + sx), f);
            var r2 = Vector3.Lerp(get_pixel(span, offset + sy), get_pixel(span, offset + sy + sx), f);
            result = Vector3.Lerp(r1, r2, y - y1);

            return result;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static Vector3 get_pixel(ReadOnlySpan<byte> span, int offset) {
            var result = new Vector3(span[offset], span[offset + 1], span[offset + 2]);
            return result;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static Vector3 get_pixel(ReadOnlySpan<byte> span, int stride, int x, int y) {
            var offset = y * stride + x * 3;
            var result = get_pixel(span, offset);
            return result;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void set_pixel(Span<byte> span, int stride, int x, int y, Vector3 value) {
            var offset = y * stride + x * 3;
            span[offset] = (byte)value.X;
            span[offset + 1] = (byte)value.Y;
            span[offset + 2] = (byte)value.Z;
        }

        private static string print(Image img) {
            var sb = new StringBuilder();
            sb.Append("{");
            for (var i = 0; i < img.Height; i++) {
                var empty = true;
                for (var j = 0; j < img.Width; j++) {
                    var p = img.GetPixel(j, i);
                    if (p.r != 0 || p.g != 0 || p.b != 0) {
                        empty = false;
                        break;
                    }
                }
                if (empty) {
                    continue;
                }
                sb.Append($"r{i}:{{");
                for (var j = 0; j < img.Width; j++) {
                    var p = img.GetPixel(j, i);
                    if (p.r == 0 && p.g == 0 && p.b == 0) {
                        continue;
                    }
                    sb.Append($"c{j}:[{p.r},{p.g},{p.b}],");
                }
                sb.AppendLine("},");
            }
            sb.Append("}");
            return sb.ToString();
        }

        internal static string print(ref TwoDims<Vector3> img) {
            var sb = new StringBuilder();
            sb.Append("{");
            for (var i = 0; i < img.Rows; i++) {
                var empty = true;
                for (var j = 0; j < img.Columns; j++) {
                    var p = img[i, j];
                    if (p.X != 0 || p.Y != 0 || p.Z != 0) {
                        empty = false;
                        break;
                    }
                }
                if (empty) {
                    continue;
                }
                sb.Append($"r{i}:{{");
                for (var j = 0; j < img.Columns; j++) {
                    var p = img[i, j];
                    if (p.X == 0 && p.Y == 0 && p.Z == 0) {
                        continue;
                    }
                    sb.Append($"c{j}:[{p.X},{p.Y},{p.Z}],");
                }
                sb.AppendLine("},");
            }
            sb.Append("}");
            return sb.ToString();
        }

        internal static bool is_close(TwoDims<Vector3> left, TwoDims<Vector3> right) {
            if (left.Rows != right.Rows || left.Columns != right.Columns) {
                return false;
            }
            for (var i = 0; i < left.Rows; i++) {
                for (var j = 0; j < left.Columns; j++) {
                    var l = left[i, j];
                    var r = right[i, j];
                    if (Math.Abs(l.X - r.X) > 0.0001) {
                        return false;
                    }
                    if (Math.Abs(l.Y - r.Y) > 0.0001) {
                        return false;
                    }
                    if (Math.Abs(l.Z - r.Z) > 0.0001) {
                        return false;
                    }
                }
            }
            return true;
        }

        internal static float mean_absolute_error(in Image left, in Image right) {
            Debug.Assert(left.PixelFormat == PixelFormat.RGB_24bpp);
            Debug.Assert(right.PixelFormat == PixelFormat.RGB_24bpp);
            if (left.Width != right.Width || left.Height != right.Height) {
                return float.PositiveInfinity;
            }
            var total = 0L;
            for (var i = 0; i < left.Height; i++) {
                for (var j = 0; j < left.Width; j++) {
                    var l = left.GetPixel(j, i);
                    var r = right.GetPixel(j, i);
                    total += Math.Abs(l.r - r.r);
                    total += Math.Abs(l.g - r.g);
                    total += Math.Abs(l.b - r.b);
                }
            }
            var result = total / (left.Width * left.Height * 3f);
            return result;
        }
#endregion

    }
}
