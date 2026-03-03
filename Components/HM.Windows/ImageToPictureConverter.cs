using System;
using System.Buffers;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using HMInterop;
using Microsoft.Psi;
using Microsoft.Psi.Imaging;

namespace OpenSense.Components.HM {
    public sealed class ImageToPictureConverter : IConsumer<Shared<Image>>, IProducer<Shared<PicYuv>>, INotifyPropertyChanged, IDisposable {

        #region Ports
        public Receiver<Shared<Image>> In { get; }

        public Emitter<Shared<PicYuv>> Out { get; }
        #endregion

        #region Options
        private ChromaFormat chromaFormat = ChromaFormat.Chroma400;

        public ChromaFormat ChromaFormat {
            get => chromaFormat;
            set => SetProperty(ref chromaFormat, value);
        }

        private int outputBitDepth = 16;

        public int OutputBitDepth {
            get => outputBitDepth;
            set => SetProperty(ref outputBitDepth, value);
        }
        #endregion

        private int initializedWidth;
        private int initializedHeight;
        private bool initialized;

        public ImageToPictureConverter(Pipeline pipeline) {
            In = pipeline.CreateReceiver<Shared<Image>>(this, Process, nameof(In));
            Out = pipeline.CreateEmitter<Shared<PicYuv>>(this, nameof(Out));
        }

        private void Process(Shared<Image> image, Envelope envelope) {
            var resource = image.Resource;
            var width = resource.Width;
            var height = resource.Height;

            if (!initialized) {
                initializedWidth = width;
                initializedHeight = height;
                initialized = true;
            } else {
                if (width != initializedWidth || height != initializedHeight) {
                    throw new InvalidOperationException($"Image size changed: expected {initializedWidth}x{initializedHeight}, got {width}x{height}.");
                }
            }

            var chromaFmt = ChromaFormat;
            var bitDepth = OutputBitDepth;
            if (bitDepth < 8 || bitDepth > PicYuv.MaxBitDepth) {
                throw new InvalidOperationException($"OutputBitDepth must be between 8 and {PicYuv.MaxBitDepth}, but was {bitDepth}.");
            }
            var picture = new PicYuv(chromaFmt, width, height);

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
                    picture.Dispose();
                    throw new NotSupportedException($"Pixel format {resource.PixelFormat} is not supported.");
            }

            using var shared = Shared.Create(picture);
            Out.Post(shared, envelope.OriginatingTime);
        }

        private static unsafe void ConvertGray8(Image image, PicYuv picture, ChromaFormat chromaFmt, int bitDepth) {
            var width = image.Width;
            var height = image.Height;
            var stride = image.Stride;
            var pixelCount = width * height;
            // HM CopyYPlane always reads uint16_t
            var bufferSize = pixelCount * sizeof(ushort);
            var buffer = ArrayPool<byte>.Shared.Rent(bufferSize);
            try {
                var src = new ReadOnlySpan<byte>(image.ImageData.ToPointer(), stride * height);
                fixed (byte* bufPtr = buffer) {
                    var dst = new Span<ushort>(bufPtr, pixelCount);
                    var shift = bitDepth - 8;
                    for (var y = 0; y < height; y++) {
                        var srcRow = src.Slice(y * stride, width);
                        var dstOffset = y * width;
                        for (var x = 0; x < width; x++) {
                            dst[dstOffset + x] = (ushort)(srcRow[x] << shift);
                        }
                    }
                    picture.CopyYPlane(new IntPtr(bufPtr), bufferSize);
                }
                if (chromaFmt != ChromaFormat.Chroma400) {
                    FillNeutralChroma(picture, chromaFmt, width, height, bitDepth);
                }
            } finally {
                ArrayPool<byte>.Shared.Return(buffer);
            }
        }

        private static unsafe void ConvertGray16(Image image, PicYuv picture, ChromaFormat chromaFmt, int bitDepth) {
            var width = image.Width;
            var height = image.Height;
            var stride = image.Stride;
            var bytesPerRow = width * sizeof(ushort);
            // Check if stride matches packed layout
            if (stride == bytesPerRow) {
                // Direct copy, no stride padding
                if (bitDepth == 16) {
                    picture.CopyYPlane(image.ImageData, width * height * sizeof(ushort));
                } else {
                    // Need to narrow 16-bit to target bit depth
                    var pixelCount = width * height;
                    var bufferSize = pixelCount * sizeof(ushort);
                    var buffer = ArrayPool<byte>.Shared.Rent(bufferSize);
                    try {
                        var src = new ReadOnlySpan<ushort>(image.ImageData.ToPointer(), pixelCount);
                        fixed (byte* bufPtr = buffer) {
                            var dst = new Span<ushort>(bufPtr, pixelCount);
                            var shift = 16 - bitDepth;
                            for (var i = 0; i < pixelCount; i++) {
                                dst[i] = (ushort)(src[i] >> shift);
                            }
                            picture.CopyYPlane(new IntPtr(bufPtr), bufferSize);
                        }
                    } finally {
                        ArrayPool<byte>.Shared.Return(buffer);
                    }
                }
            } else {
                // Strip stride padding
                var pixelCount = width * height;
                var bufferSize = pixelCount * sizeof(ushort);
                var buffer = ArrayPool<byte>.Shared.Rent(bufferSize);
                try {
                    var src = new ReadOnlySpan<byte>(image.ImageData.ToPointer(), stride * height);
                    fixed (byte* bufPtr = buffer) {
                        var dst = new Span<ushort>(bufPtr, pixelCount);
                        var shift = 16 - bitDepth;
                        for (var y = 0; y < height; y++) {
                            var srcRow = new ReadOnlySpan<ushort>((byte*)image.ImageData.ToPointer() + y * stride, width);
                            var dstOffset = y * width;
                            if (bitDepth == 16) {
                                srcRow.CopyTo(dst.Slice(dstOffset, width));
                            } else {
                                for (var x = 0; x < width; x++) {
                                    dst[dstOffset + x] = (ushort)(srcRow[x] >> shift);
                                }
                            }
                        }
                        picture.CopyYPlane(new IntPtr(bufPtr), bufferSize);
                    }
                } finally {
                    ArrayPool<byte>.Shared.Return(buffer);
                }
            }
            if (chromaFmt != ChromaFormat.Chroma400) {
                FillNeutralChroma(picture, chromaFmt, width, height, bitDepth);
            }
        }

        private static unsafe void ConvertColor(Image image, PicYuv picture, ChromaFormat chromaFmt, int bitDepth, int rOffset, int gOffset, int bOffset, int bytesPerPixel) {
            var width = image.Width;
            var height = image.Height;
            var stride = image.Stride;
            var pixelCount = width * height;
            var planeBytes = pixelCount * sizeof(ushort);

            // Allocate Y plane buffer (always needed)
            var yBuffer = ArrayPool<byte>.Shared.Rent(planeBytes);
            // Allocate full-resolution Cb/Cr buffers if color output
            byte[]? cbBuffer = null;
            byte[]? crBuffer = null;
            if (chromaFmt != ChromaFormat.Chroma400) {
                cbBuffer = ArrayPool<byte>.Shared.Rent(planeBytes);
                crBuffer = ArrayPool<byte>.Shared.Rent(planeBytes);
            }

            try {
                var src = new ReadOnlySpan<byte>(image.ImageData.ToPointer(), stride * height);
                var shift = bitDepth > 8 ? bitDepth - 8 : 0;

                fixed (byte* yPtr = yBuffer) {
                    var yDst = new Span<ushort>(yPtr, pixelCount);

                    if (chromaFmt == ChromaFormat.Chroma400) {
                        // Only compute Y (luma)
                        for (var y = 0; y < height; y++) {
                            var srcRow = src.Slice(y * stride, width * bytesPerPixel);
                            var dstOffset = y * width;
                            for (var x = 0; x < width; x++) {
                                var px = x * bytesPerPixel;
                                int r = srcRow[px + rOffset];
                                int g = srcRow[px + gOffset];
                                int b = srcRow[px + bOffset];
                                var yVal = Clamp((66 * r + 129 * g + 25 * b + 128) >> 8, 0, 219) + 16;
                                yDst[dstOffset + x] = (ushort)(yVal << shift);
                            }
                        }
                        picture.CopyYPlane(new IntPtr(yPtr), planeBytes);
                    } else {
                        // Compute full YCbCr at 444
                        fixed (byte* cbPtr = cbBuffer, crPtr = crBuffer) {
                            var cbDst = new Span<ushort>(cbPtr, pixelCount);
                            var crDst = new Span<ushort>(crPtr, pixelCount);

                            for (var y = 0; y < height; y++) {
                                var srcRow = src.Slice(y * stride, width * bytesPerPixel);
                                var dstOffset = y * width;
                                for (var x = 0; x < width; x++) {
                                    var px = x * bytesPerPixel;
                                    int r = srcRow[px + rOffset];
                                    int g = srcRow[px + gOffset];
                                    int b = srcRow[px + bOffset];
                                    var yVal = Clamp((66 * r + 129 * g + 25 * b + 128) >> 8, 0, 219) + 16;
                                    var cbVal = Clamp((-38 * r - 74 * g + 112 * b + 128) >> 8, -112, 112) + 128;
                                    var crVal = Clamp((112 * r - 94 * g - 18 * b + 128) >> 8, -112, 112) + 128;
                                    yDst[dstOffset + x] = (ushort)(yVal << shift);
                                    cbDst[dstOffset + x] = (ushort)(cbVal << shift);
                                    crDst[dstOffset + x] = (ushort)(crVal << shift);
                                }
                            }

                            picture.CopyYPlane(new IntPtr(yPtr), planeBytes);

                            if (chromaFmt == ChromaFormat.Chroma444) {
                                picture.CopyUPlane(new IntPtr(cbPtr), planeBytes);
                                picture.CopyVPlane(new IntPtr(crPtr), planeBytes);
                            } else {
                                // Subsample chroma
                                GetChromaDimensions(chromaFmt, width, height, out var chromaW, out var chromaH);
                                var chromaPixels = chromaW * chromaH;
                                var chromaBytes = chromaPixels * sizeof(ushort);
                                var subCbBuf = ArrayPool<byte>.Shared.Rent(chromaBytes);
                                var subCrBuf = ArrayPool<byte>.Shared.Rent(chromaBytes);
                                try {
                                    fixed (byte* subCbPtr = subCbBuf, subCrPtr = subCrBuf) {
                                        var subCb = new Span<ushort>(subCbPtr, chromaPixels);
                                        var subCr = new Span<ushort>(subCrPtr, chromaPixels);
                                        Subsample(cbDst, width, height, subCb, chromaW, chromaH, chromaFmt);
                                        Subsample(crDst, width, height, subCr, chromaW, chromaH, chromaFmt);
                                        picture.CopyUPlane(new IntPtr(subCbPtr), chromaBytes);
                                        picture.CopyVPlane(new IntPtr(subCrPtr), chromaBytes);
                                    }
                                } finally {
                                    ArrayPool<byte>.Shared.Return(subCbBuf);
                                    ArrayPool<byte>.Shared.Return(subCrBuf);
                                }
                            }
                        }
                    }
                }
            } finally {
                ArrayPool<byte>.Shared.Return(yBuffer);
                if (cbBuffer is not null) {
                    ArrayPool<byte>.Shared.Return(cbBuffer);
                }
                if (crBuffer is not null) {
                    ArrayPool<byte>.Shared.Return(crBuffer);
                }
            }
        }

        private static unsafe void FillNeutralChroma(PicYuv picture, ChromaFormat chromaFmt, int width, int height, int bitDepth) {
            GetChromaDimensions(chromaFmt, width, height, out var chromaW, out var chromaH);
            var chromaPixels = chromaW * chromaH;
            var chromaBytes = chromaPixels * sizeof(ushort);
            var buffer = ArrayPool<byte>.Shared.Rent(chromaBytes);
            try {
                fixed (byte* bufPtr = buffer) {
                    var dst = new Span<ushort>(bufPtr, chromaPixels);
                    var neutralValue = (ushort)(128 << (bitDepth - 8));
                    dst.Slice(0, chromaPixels).Fill(neutralValue);
                    picture.CopyUPlane(new IntPtr(bufPtr), chromaBytes);
                    picture.CopyVPlane(new IntPtr(bufPtr), chromaBytes);
                }
            } finally {
                ArrayPool<byte>.Shared.Return(buffer);
            }
        }

        private static void GetChromaDimensions(ChromaFormat chromaFmt, int lumaW, int lumaH, out int chromaW, out int chromaH) {
            switch (chromaFmt) {
                case ChromaFormat.Chroma420:
                    chromaW = (lumaW + 1) / 2;
                    chromaH = (lumaH + 1) / 2;
                    break;
                case ChromaFormat.Chroma422:
                    chromaW = (lumaW + 1) / 2;
                    chromaH = lumaH;
                    break;
                case ChromaFormat.Chroma444:
                    chromaW = lumaW;
                    chromaH = lumaH;
                    break;
                default:
                    chromaW = 0;
                    chromaH = 0;
                    break;
            }
        }

        private static void Subsample(ReadOnlySpan<ushort> src, int srcW, int srcH, Span<ushort> dst, int dstW, int dstH, ChromaFormat chromaFmt) {
            if (chromaFmt == ChromaFormat.Chroma420) {
                // 2x2 block average
                for (var cy = 0; cy < dstH; cy++) {
                    var sy = cy * 2;
                    var sy1 = Math.Min(sy + 1, srcH - 1);
                    for (var cx = 0; cx < dstW; cx++) {
                        var sx = cx * 2;
                        var sx1 = Math.Min(sx + 1, srcW - 1);
                        var sum = (int)src[sy * srcW + sx] + src[sy * srcW + sx1] + src[sy1 * srcW + sx] + src[sy1 * srcW + sx1];
                        dst[cy * dstW + cx] = (ushort)((sum + 2) / 4);
                    }
                }
            } else if (chromaFmt == ChromaFormat.Chroma422) {
                // 2x1 horizontal average
                for (var cy = 0; cy < dstH; cy++) {
                    for (var cx = 0; cx < dstW; cx++) {
                        var sx = cx * 2;
                        var sx1 = Math.Min(sx + 1, srcW - 1);
                        var sum = (int)src[cy * srcW + sx] + src[cy * srcW + sx1];
                        dst[cy * dstW + cx] = (ushort)((sum + 1) / 2);
                    }
                }
            }
        }

        private static int Clamp(int value, int min, int max) {
            if (value < min) {
                return min;
            }
            if (value > max) {
                return max;
            }
            return value;
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
