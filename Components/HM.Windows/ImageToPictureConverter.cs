using System;
using System.Buffers;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using HMInterop;
using Microsoft.Psi;
using Microsoft.Psi.Imaging;

namespace OpenSense.Components.HM {
    public sealed class ImageToPictureConverter : IConsumer<Shared<Image>>, IProducer<Shared<PictureSnapshot>>, INotifyPropertyChanged, IDisposable {

        #region Ports
        public Receiver<Shared<Image>> In { get; }

        public Emitter<Shared<PictureSnapshot>> Out { get; }
        #endregion

        #region Options

        #region Input
        private PixelFormat? inputPixelFormat;

        public PixelFormat? InputPixelFormat {
            get => inputPixelFormat;
            set => SetProperty(ref inputPixelFormat, value);
        }

        private int sourceBitDepth;

        public int SourceBitDepth {
            get => sourceBitDepth;
            set => SetProperty(ref sourceBitDepth, value);
        }
        #endregion

        #region Bit Depth Mapping
        private int bitDepthMappingScaleShift;

        public int BitDepthMappingScaleShift {
            get => bitDepthMappingScaleShift;
            set => SetProperty(ref bitDepthMappingScaleShift, value);
        }

        private int bitDepthMappingWindow;

        public int BitDepthMappingWindow {
            get => bitDepthMappingWindow;
            set => SetProperty(ref bitDepthMappingWindow, value);
        }
        #endregion

        #region Output
        private int outputBitDepth = 16;

        public int OutputBitDepth {
            get => outputBitDepth;
            set => SetProperty(ref outputBitDepth, value);
        }

        private ChromaFormat outputChromaFormat = ChromaFormat.Chroma400;

        public ChromaFormat OutputChromaFormat {
            get => outputChromaFormat;
            set => SetProperty(ref outputChromaFormat, value);
        }
        #endregion

        #endregion

        private int initializedWidth;
        private int initializedHeight;
        private bool initialized;
        private int pocCounter;

        public ImageToPictureConverter(Pipeline pipeline) {
            In = pipeline.CreateReceiver<Shared<Image>>(this, Process, nameof(In));
            Out = pipeline.CreateEmitter<Shared<PictureSnapshot>>(this, nameof(Out));
        }

        private void Process(Shared<Image> image, Envelope envelope) {
            var resource = image.Resource;
            var width = resource.Width;
            var height = resource.Height;

            if (!initialized) {
                initializedWidth = width;
                initializedHeight = height;
                initialized = true;

                // Validate input pixel format
                if (InputPixelFormat is { } expected && resource.PixelFormat != expected) {
                    throw new InvalidOperationException($"Expected pixel format {expected} but got {resource.PixelFormat}.");
                }
            } else {
                if (width != initializedWidth || height != initializedHeight) {
                    throw new InvalidOperationException($"Image size changed: expected {initializedWidth}x{initializedHeight}, got {width}x{height}.");
                }
            }

            // Detect source bit depth from pixel format
            var detectedBits = resource.PixelFormat switch {
                PixelFormat.Gray_16bpp => 16,
                _ => 8,
            };
            if (SourceBitDepth > 0 && detectedBits != SourceBitDepth) {
                throw new InvalidOperationException($"SourceBitDepth is set to {SourceBitDepth} but input pixel format {resource.PixelFormat} implies {detectedBits}-bit.");
            }

            var chromaFmt = OutputChromaFormat;
            var bitDepth = OutputBitDepth;
            if (bitDepth < 8 || bitDepth > 16) {
                throw new InvalidOperationException($"Output bit depth must be between 8 and 16 (HEVC supported range), but was {bitDepth}.");
            }
            var picture = PictureYuvPool.Rent(chromaFmt, width, height);

            switch (resource.PixelFormat) {
                case PixelFormat.Gray_8bpp:
                    ConvertGray8(resource, picture, chromaFmt, bitDepth);
                    break;
                case PixelFormat.Gray_16bpp:
                    ConvertGray16(resource, picture, chromaFmt, bitDepth);
                    break;
                case PixelFormat.BGR_24bpp:
                    ConvertColor(resource, picture, chromaFmt, bitDepth, rOffset: 2, gOffset: 1, bOffset: 0, bytesPerPixel: 3);
                    break;
                case PixelFormat.RGB_24bpp:
                    ConvertColor(resource, picture, chromaFmt, bitDepth, rOffset: 0, gOffset: 1, bOffset: 2, bytesPerPixel: 3);
                    break;
                case PixelFormat.BGRA_32bpp:
                case PixelFormat.BGRX_32bpp:
                    ConvertColor(resource, picture, chromaFmt, bitDepth, rOffset: 2, gOffset: 1, bOffset: 0, bytesPerPixel: 4);
                    break;
                default:
                    throw new NotSupportedException($"Pixel format {resource.PixelFormat} is not supported.");
            }

            var sps = new SequenceParameterSetSnapshot(
                new BitDepths { Luma = bitDepth, Chroma = bitDepth },
                chromaFmt, width, height);
            var snapshot = new PictureSnapshot(picture, pocCounter++, sps, PictureYuvOwnership.Pooled);
            using var shared = Shared.Create(snapshot);
            Out.Post(shared, envelope.OriginatingTime);
        }

        private static unsafe void ConvertGray8(Image image, PictureYuv picture, ChromaFormat chromaFmt, int bitDepth) {
            var width = image.Width;
            var height = image.Height;
            var imgStride = image.Stride;
            var src = new ReadOnlySpan<byte>(image.ImageData.ToPointer(), imgStride * height);

            var (yPtr, yW, yH, yStride) = picture.GetPlaneAccess(ComponentId.Y);
            var yPels = new Span<int>(yPtr.ToPointer(), yStride * yH);
            var shift = bitDepth - 8;
            for (var y = 0; y < height; y++) {
                var srcRow = src.Slice(y * imgStride, width);
                var dstRow = yPels.Slice(y * yStride, width);
                for (var x = 0; x < width; x++) {
                    dstRow[x] = srcRow[x] << shift;
                }
            }

            if (chromaFmt != ChromaFormat.Chroma400) {
                picture.FillNeutralChroma(bitDepth);
            }
        }

        private unsafe void ConvertGray16(Image image, PictureYuv picture, ChromaFormat chromaFmt, int bitDepth) {
            var width = image.Width;
            var height = image.Height;
            var imgStride = image.Stride;

            var (yPtr, yW, yH, yStride) = picture.GetPlaneAccess(ComponentId.Y);
            var yPels = new Span<int>(yPtr.ToPointer(), yStride * yH);

            for (var y = 0; y < height; y++) {
                var srcRow = new ReadOnlySpan<ushort>((byte*)image.ImageData.ToPointer() + y * imgStride, width);
                var dstRow = yPels.Slice(y * yStride, width);
                for (var x = 0; x < width; x++) {
                    dstRow[x] = srcRow[x];
                }
            }

            // Apply bit depth mapping only when actual transformation is needed
            if (BitDepthMappingScaleShift != 0 || BitDepthMappingWindow != 0 || bitDepth != 16) {
                BitDepthMapper.MapPlane(yPels, yW, yH, yStride, bitDepth, BitDepthMappingScaleShift, BitDepthMappingWindow);
            }

            if (chromaFmt != ChromaFormat.Chroma400) {
                picture.FillNeutralChroma(bitDepth);
            }
        }

        private static unsafe void ConvertColor(Image image, PictureYuv picture, ChromaFormat chromaFmt, int bitDepth, int rOffset, int gOffset, int bOffset, int bytesPerPixel) {
            var width = image.Width;
            var height = image.Height;
            var imgStride = image.Stride;
            var src = new ReadOnlySpan<byte>(image.ImageData.ToPointer(), imgStride * height);
            var shift = bitDepth > 8 ? bitDepth - 8 : 0;

            var (yPtr, yW, yH, yStride) = picture.GetPlaneAccess(ComponentId.Y);
            var yPels = new Span<int>(yPtr.ToPointer(), yStride * yH);

            if (chromaFmt == ChromaFormat.Chroma400) {
                // Only compute Y (luma), write directly to Pel buffer
                for (var y = 0; y < height; y++) {
                    var srcRow = src.Slice(y * imgStride, width * bytesPerPixel);
                    var yRow = yPels.Slice(y * yStride, width);
                    for (var x = 0; x < width; x++) {
                        var px = x * bytesPerPixel;
                        var r = (int)srcRow[px + rOffset];
                        var g = (int)srcRow[px + gOffset];
                        var b = (int)srcRow[px + bOffset];
                        var yVal = Math.Clamp((66 * r + 129 * g + 25 * b + 128) >> 8, 0, 219) + 16;
                        yRow[x] = yVal << shift;
                    }
                }
            } else {
                // Compute full YCbCr, write Y/Cb/Cr directly to Pel buffers
                var (cbPtr, cbW, cbH, cbStride) = picture.GetPlaneAccess(ComponentId.Cb);
                var (crPtr, crW, crH, crStride) = picture.GetPlaneAccess(ComponentId.Cr);

                if (chromaFmt == ChromaFormat.Chroma444) {
                    // 444: all planes same resolution, write directly
                    var cbPels = new Span<int>(cbPtr.ToPointer(), cbStride * cbH);
                    var crPels = new Span<int>(crPtr.ToPointer(), crStride * crH);

                    for (var y = 0; y < height; y++) {
                        var srcRow = src.Slice(y * imgStride, width * bytesPerPixel);
                        var yRow = yPels.Slice(y * yStride, width);
                        var cbRow = cbPels.Slice(y * cbStride, width);
                        var crRow = crPels.Slice(y * crStride, width);
                        for (var x = 0; x < width; x++) {
                            var px = x * bytesPerPixel;
                            var rv = (int)srcRow[px + rOffset];
                            var gv = (int)srcRow[px + gOffset];
                            var bv = (int)srcRow[px + bOffset];
                            var yVal = Math.Clamp((66 * rv + 129 * gv + 25 * bv + 128) >> 8, 0, 219) + 16;
                            var cbVal = Math.Clamp((-38 * rv - 74 * gv + 112 * bv + 128) >> 8, -112, 112) + 128;
                            var crVal = Math.Clamp((112 * rv - 94 * gv - 18 * bv + 128) >> 8, -112, 112) + 128;
                            yRow[x] = yVal << shift;
                            cbRow[x] = cbVal << shift;
                            crRow[x] = crVal << shift;
                        }
                    }
                } else {
                    // 420/422: compute at 444, subsample chroma, write to Pel buffers
                    var pixelCount = width * height;
                    var cbBuf = ArrayPool<int>.Shared.Rent(pixelCount);
                    var crBuf = ArrayPool<int>.Shared.Rent(pixelCount);
                    try {
                        var fullCb = cbBuf.AsSpan(0, pixelCount);
                        var fullCr = crBuf.AsSpan(0, pixelCount);

                        for (var y = 0; y < height; y++) {
                            var srcRow = src.Slice(y * imgStride, width * bytesPerPixel);
                            var yRow = yPels.Slice(y * yStride, width);
                            for (var x = 0; x < width; x++) {
                                var px = x * bytesPerPixel;
                                int rv = srcRow[px + rOffset];
                                int gv = srcRow[px + gOffset];
                                int bv = srcRow[px + bOffset];
                                var yVal = Math.Clamp((66 * rv + 129 * gv + 25 * bv + 128) >> 8, 0, 219) + 16;
                                var cbVal = Math.Clamp((-38 * rv - 74 * gv + 112 * bv + 128) >> 8, -112, 112) + 128;
                                var crVal = Math.Clamp((112 * rv - 94 * gv - 18 * bv + 128) >> 8, -112, 112) + 128;
                                yRow[x] = yVal << shift;
                                fullCb[y * width + x] = cbVal << shift;
                                fullCr[y * width + x] = crVal << shift;
                            }
                        }

                        // Subsample and write directly to chroma Pel buffers
                        var cbPels = new Span<int>(cbPtr.ToPointer(), cbStride * cbH);
                        var crPels = new Span<int>(crPtr.ToPointer(), crStride * crH);
                        ChromaConverter.SubsampleToPels(fullCb, width, height, cbPels, cbW, cbH, cbStride, chromaFmt);
                        ChromaConverter.SubsampleToPels(fullCr, width, height, crPels, crW, crH, crStride, chromaFmt);
                    } finally {
                        ArrayPool<int>.Shared.Return(crBuf);
                        ArrayPool<int>.Shared.Return(cbBuf);
                    }
                }
            }
        }

        #region IDisposable
        private bool disposed;

        public void Dispose() {
            if (disposed) {
                return;
            }
            disposed = true;
        }
        #endregion

        #region INotifyPropertyChanged
        public event PropertyChangedEventHandler? PropertyChanged;

        private void SetProperty<T>(ref T field, T value, [CallerMemberName] string? propertyName = null) {
            if (!EqualityComparer<T>.Default.Equals(field, value)) {
                field = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            }
        }
        #endregion
    }
}
