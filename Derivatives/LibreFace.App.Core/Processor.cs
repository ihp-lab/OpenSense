using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text.Json;
using FFMpegInterop;
using Mediapipe.Net.Framework;
using Mediapipe.Net.Framework.Format;
using Mediapipe.Net.Framework.Packets;
using Mediapipe.Net.Framework.Port;
using Mediapipe.Net.Framework.Protobuf;
using Microsoft.ML.OnnxRuntime;
using Microsoft.Psi;
using Microsoft.Psi.Imaging;
using OpenSense.Components.LibreFace;

namespace LibreFace.App {
    public sealed class Processor : IDisposable {

        private const string GraphPath = "mediapipe/modules/face_landmark/face_landmark_front_cpu.pbtxt";

        private static readonly PixelFormat PsiPxielFormat = PixelFormat.RGB_24bpp;

        private static readonly int MediaPipePixelFormat = ToFFMpegPixelFormat(PsiPxielFormat);

        private static readonly PacketType DesiredType = PacketType.NormalizedLandmarkListVector;

        private static readonly TypeSetterMethod SetPacketTypeMethod;

        private static readonly RunMethod AlignMethod;

        private readonly IProgress<TimeSpan> _progress;

        private readonly string _outFilename;

        private readonly FileReader _reader;

        private readonly CalculatorGraph _graph;

        private readonly GCHandle _handle;

        private readonly SidePackets _sidePackets;

        private Image image = new Image(0, 0, PsiPxielFormat);

        private TimeSpan frameTime = TimeSpan.Zero;

        private List<NormalizedLandmarkList>? landmarks;

        private TimeSpan landmarkTime = TimeSpan.Zero;

        static Processor() {
            var method1 = typeof(Packet).GetProperty(nameof(Packet.PacketType), BindingFlags.Instance | BindingFlags.Public)
                ?.GetSetMethod(nonPublic: true)
                ?? throw new InvalidOperationException($"Cound not find the setter method of {nameof(Packet)}.{nameof(Packet.PacketType)} property.");
            SetPacketTypeMethod = (TypeSetterMethod)Delegate.CreateDelegate(typeof(TypeSetterMethod), method1);

            var method2 = typeof(FaceImageAligner).GetMethod("Run", BindingFlags.Static | BindingFlags.NonPublic)
                ?? throw new InvalidOperationException($"Cound not find the method of {nameof(FaceImageAligner)}.Run.");
            AlignMethod = (RunMethod)Delegate.CreateDelegate(typeof(RunMethod), method2);
        }

        public Processor(string filename, string outDir, int numFaces, IProgress<TimeSpan> progress) {
            _progress = progress;
            var stem = Path.GetFileNameWithoutExtension(filename);
            _outFilename = Path.Combine(outDir, stem + ".json");

            _reader = new FileReader(filename) {
                TargetFormat = ToFFMpegPixelFormat(PsiPxielFormat),
            };

            var graph = File.ReadAllText(GraphPath);
            var config = CalculatorGraphConfig.Parser.ParseFromTextFormat(graph);
            _graph = new CalculatorGraph(config);
            _graph.ObserveOutputStream("multi_face_landmarks", PacketCallback, out _handle).AssertOk();//not using poller, because there is not way to create an empty packet.
            _sidePackets = new SidePackets();
            _sidePackets.Emplace("use_prev_landmarks", PacketFactory.BoolPacket(true));
            _sidePackets.Emplace("num_faces", PacketFactory.IntPacket(numFaces));
            _sidePackets.Emplace("with_attention", PacketFactory.BoolPacket(false));
        }

        public void Run() {
            var counter = 0;

            _graph.StartRun(_sidePackets).AssertOk();

            using var stream = new FileStream(_outFilename, FileMode.Create, FileAccess.Write, FileShare.None);
            using var writer = new Utf8JsonWriter(stream, LibreFaceJsonWriter.Options);
            LibreFaceJsonWriter.WriteStart(writer);

            using var options = SessionOptions.MakeSessionOptionWithCudaProvider(deviceId: 0);
            using var pContext = new ActionUnitPresenceModelContext(options, isOwner: true);
            using var iContext = new ActionUnitIntensityModelContext(options, isOwner: true);
            using var fContext = new FacialExpressionModelContext(options, isOwner: true);
            var pResults = new List<ActionUnitPresenceOutput>();
            var iResults = new List<ActionUnitIntensityOutput>();
            var fResults = new List<ExpressionOutput>();

            var eof = false;
            while (!eof) {
                _reader.ReadOneFrame(OnAllocate, out var valid, out eof);
                if (!valid) {
                    continue;
                }

                if (landmarks is not null) {
                    landmarks.Clear();
                    landmarks = null;
                }
                using var packet = ConvertImage(image, frameTime);
                _graph.AddPacketToInputStream("image", packet);
                _graph.WaitUntilIdle().AssertOk();
                pResults.Clear();
                iResults.Clear();
                fResults.Clear();
                if (landmarks is not null && landmarks.Count != 0) {
                    Debug.Assert(frameTime == landmarkTime);
                    var croppedImages = AlignMethod(image, landmarks);
                    foreach (var sharedImg in croppedImages) {
                        try {
                            var img = sharedImg.Resource;
                            unsafe {
                                var span = new Span<byte>(img.ImageData.ToPointer(), img.Size);
                                using var input = new ImageInput(span, img.Stride);
                                var pResult = pContext.Run(input);
                                var iResult = iContext.Run(input);
                                var fResult = fContext.Run(input);
                                pResults.Add(pResult);
                                iResults.Add(iResult);
                                fResults.Add(fResult);
                            }
                        } finally {
                            sharedImg.Dispose();
                        }
                    }
                }
                LibreFaceJsonWriter.WriteFrame(writer, counter, pResults, iResults, fResults, frameTime);

                counter++;
                _progress.Report(frameTime);
            }
            LibreFaceJsonWriter.WriteEnd(writer);
            _graph.CloseInputStream("image").AssertOk();
            _graph.WaitUntilDone().AssertOk();
        }

        #region FileSource Helpers
        private (IntPtr, int) OnAllocate(FrameInfo info) {
            if (info.Width != image.Width || info.Height != image.Height) {
                image.Dispose();
                image = new Image(info.Width, info.Height, PixelFormat.RGB_24bpp);
            }
            frameTime = info.Timestamp;
            var ptr = image.UnmanagedBuffer.Data;
            var length = image.UnmanagedBuffer.Size;
            return (ptr, length);
        }

        private static int ToFFMpegPixelFormat(PixelFormat psiFormat) {
            switch (psiFormat) {
                case PixelFormat.RGB_24bpp:
                    return 2;//AV_PIX_FMT_RGB24
                default:
                    throw new InvalidOperationException($"Unsupported \\psi pixel format {psiFormat}.");
            }
        }
        #endregion

        #region MediaPipe Helpers
        private static unsafe ImageFrame CreateImageFrame(Image image) {
            var span = new ReadOnlySpan<byte>(image.UnmanagedBuffer.Data.ToPointer(), image.UnmanagedBuffer.Size);//Unsafe
            var result = new ImageFrame(Mediapipe.Net.Framework.Format.ImageFormat.Srgb, image.Width, image.Height, image.Stride, span);
            return result;
        }

        internal static Packet ConvertImage(Image data, TimeSpan time) {
            var frame = CreateImageFrame(data);//Disposed by PacketFactory.ImageFramePacket
            var timestamp = new Timestamp(time.Ticks);
            var result = PacketFactory.ImageFramePacket(frame, timestamp);
            return result;
        }

        private Status PacketCallback(Packet packet) {
            SetPacketTypeMethod(packet, DesiredType);
            landmarks = (List<NormalizedLandmarkList>?)packet.Get();
            var timestamp = packet.Timestamp();
            landmarkTime = TimeSpan.FromTicks(timestamp.Value);
            return Status.Ok();
        }

        private delegate void TypeSetterMethod(Packet obj, PacketType value);
        #endregion

        #region Face Aligner
        private delegate IReadOnlyList<Shared<Image>> RunMethod(Image image, IReadOnlyList<NormalizedLandmarkList> faces);
        #endregion

        #region IDisposable

        private bool disposed;
        public void Dispose() {
            if (disposed) {
                return;
            }
            disposed = true;

            _sidePackets.Dispose();
            _graph.Dispose();
            _handle.Free();
            _reader.Dispose();
            image.Dispose();
        }
        #endregion
    }
}
