using System;
using System.Buffers;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using KvazaarInterop;
using Microsoft.Psi;
using Microsoft.Psi.Imaging;

namespace OpenSense.Components.Kvazaar {
    public sealed class ImageToPictureConverter : IConsumer<Shared<Image>>, IProducer<Shared<Picture>>, INotifyPropertyChanged, IDisposable {

        #region Ports
        public Receiver<Shared<Image>> In { get; }

        public Emitter<Shared<Picture>> Out { get; }
        #endregion

        #region Options
        private ChromaFormat chromaFormat = ChromaFormat.Csp400;

        public ChromaFormat ChromaFormat {
            get => chromaFormat;
            set => SetProperty(ref chromaFormat, value);
        }

        private int outputBitDepth = 8;

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
            Out = pipeline.CreateEmitter<Shared<Picture>>(this, nameof(Out));
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
            if (bitDepth < 8 || bitDepth > Picture.MaxBitDepth) {
                throw new InvalidOperationException($"OutputBitDepth must be between 8 and {Picture.MaxBitDepth}, but was {bitDepth}.");
            }
            var nativePixelSize = Picture.MaxBitDepth / 8;
            var picture = new Picture(chromaFmt, width, height);
            var pictureStride = picture.Stride;

            switch (resource.PixelFormat) {
                case PixelFormat.Gray_8bpp:
                    ConvertGray8(resource, picture, chromaFmt, bitDepth, pictureStride, nativePixelSize);
                    break;
                case PixelFormat.Gray_16bpp:
                    ConvertGray16(resource, picture, chromaFmt, bitDepth, pictureStride, nativePixelSize);
                    break;
                case PixelFormat.BGR_24bpp:
                    ConvertColor(resource, picture, chromaFmt, bitDepth, pictureStride, nativePixelSize, rOffset: 2, gOffset: 1, bOffset: 0, bytesPerPixel: 3);
                    break;
                case PixelFormat.RGB_24bpp:
                    ConvertColor(resource, picture, chromaFmt, bitDepth, pictureStride, nativePixelSize, rOffset: 0, gOffset: 1, bOffset: 2, bytesPerPixel: 3);
                    break;
                case PixelFormat.BGRA_32bpp:
                case PixelFormat.BGRX_32bpp:
                    ConvertColor(resource, picture, chromaFmt, bitDepth, pictureStride, nativePixelSize, rOffset: 2, gOffset: 1, bOffset: 0, bytesPerPixel: 4);
                    break;
                default:
                    picture.Dispose();
                    throw new NotSupportedException($"Pixel format {resource.PixelFormat} is not supported.");
            }

            using var shared = Shared.Create(picture);
            Out.Post(shared, envelope.OriginatingTime);
        }

        private static unsafe void ConvertGray8(Image image, Picture picture, ChromaFormat chromaFmt, int bitDepth, int pictureStride, int nativePixelSize) {
            var width = image.Width;
            var height = image.Height;
            var imageStride = image.Stride;
            var yBufferSize = pictureStride * height * nativePixelSize;
            var buffer = ArrayPool<byte>.Shared.Rent(yBufferSize);
            try {
                var src = new ReadOnlySpan<byte>(image.ImageData.ToPointer(), imageStride * height);
                fixed (byte* bufPtr = buffer) {
                    if (nativePixelSize == 1) {
                        for (var y = 0; y < height; y++) {
                            var srcRow = src.Slice(y * imageStride, width);
                            var dstRow = new Span<byte>(bufPtr + y * pictureStride, width);
                            srcRow.CopyTo(dstRow);
                        }
                    } else {
                        var dst = new Span<ushort>(bufPtr, pictureStride * height);
                        var shift = Math.Max(bitDepth - 8, 0);
                        for (var y = 0; y < height; y++) {
                            var srcRow = src.Slice(y * imageStride, width);
                            var dstOffset = y * pictureStride;
                            for (var x = 0; x < width; x++) {
                                dst[dstOffset + x] = (ushort)(srcRow[x] << shift);
                            }
                        }
                    }
                    picture.CopyYPlane(new IntPtr(bufPtr), yBufferSize);
                }
                if (chromaFmt != ChromaFormat.Csp400) {
                    FillNeutralChroma(picture, chromaFmt, pictureStride, height, bitDepth, nativePixelSize);
                }
            } finally {
                ArrayPool<byte>.Shared.Return(buffer);
            }
        }

        private static unsafe void ConvertGray16(Image image, Picture picture, ChromaFormat chromaFmt, int bitDepth, int pictureStride, int nativePixelSize) {
            var width = image.Width;
            var height = image.Height;
            var imageStride = image.Stride;
            var yBufferSize = pictureStride * height * nativePixelSize;
            var buffer = ArrayPool<byte>.Shared.Rent(yBufferSize);
            try {
                fixed (byte* bufPtr = buffer) {
                    if (nativePixelSize == 1) {
                        for (var y = 0; y < height; y++) {
                            var srcRow = new ReadOnlySpan<ushort>((byte*)image.ImageData.ToPointer() + y * imageStride, width);
                            var dstRow = new Span<byte>(bufPtr + y * pictureStride, width);
                            for (var x = 0; x < width; x++) {
                                dstRow[x] = (byte)(srcRow[x] >> 8);
                            }
                        }
                    } else {
                        var dst = new Span<ushort>(bufPtr, pictureStride * height);
                        var shift = 16 - bitDepth;
                        for (var y = 0; y < height; y++) {
                            var srcRow = new ReadOnlySpan<ushort>((byte*)image.ImageData.ToPointer() + y * imageStride, width);
                            var dstOffset = y * pictureStride;
                            if (shift == 0) {
                                srcRow.CopyTo(dst.Slice(dstOffset, width));
                            } else {
                                for (var x = 0; x < width; x++) {
                                    dst[dstOffset + x] = (ushort)(srcRow[x] >> shift);
                                }
                            }
                        }
                    }
                    picture.CopyYPlane(new IntPtr(bufPtr), yBufferSize);
                }
                if (chromaFmt != ChromaFormat.Csp400) {
                    FillNeutralChroma(picture, chromaFmt, pictureStride, height, bitDepth, nativePixelSize);
                }
            } finally {
                ArrayPool<byte>.Shared.Return(buffer);
            }
        }

        private static unsafe void ConvertColor(Image image, Picture picture, ChromaFormat chromaFmt, int bitDepth, int pictureStride, int nativePixelSize, int rOffset, int gOffset, int bOffset, int bytesPerPixel) {
            var width = image.Width;
            var height = image.Height;
            var imageStride = image.Stride;
            var pixelCount = width * height;
            var yBufferSize = pictureStride * height * nativePixelSize;

            var yBuffer = ArrayPool<byte>.Shared.Rent(yBufferSize);
            // For chroma, use packed int arrays for intermediate storage before subsampling
            int[]? cbValues = null;
            int[]? crValues = null;
            if (chromaFmt != ChromaFormat.Csp400) {
                cbValues = ArrayPool<int>.Shared.Rent(pixelCount);
                crValues = ArrayPool<int>.Shared.Rent(pixelCount);
            }

            try {
                var src = new ReadOnlySpan<byte>(image.ImageData.ToPointer(), imageStride * height);
                var shift = Math.Max(bitDepth - 8, 0);

                fixed (byte* yPtr = yBuffer) {
                    if (chromaFmt == ChromaFormat.Csp400) {
                        // Only compute Y (luma)
                        if (nativePixelSize == 1) {
                            for (var y = 0; y < height; y++) {
                                var srcRow = src.Slice(y * imageStride, width * bytesPerPixel);
                                var dstRow = new Span<byte>(yPtr + y * pictureStride, width);
                                for (var x = 0; x < width; x++) {
                                    var px = x * bytesPerPixel;
                                    int r = srcRow[px + rOffset];
                                    int g = srcRow[px + gOffset];
                                    int b = srcRow[px + bOffset];
                                    var yVal = Clamp((66 * r + 129 * g + 25 * b + 128) >> 8, 0, 219) + 16;
                                    dstRow[x] = (byte)yVal;
                                }
                            }
                        } else {
                            var yDst = new Span<ushort>(yPtr, pictureStride * height);
                            for (var y = 0; y < height; y++) {
                                var srcRow = src.Slice(y * imageStride, width * bytesPerPixel);
                                var dstOffset = y * pictureStride;
                                for (var x = 0; x < width; x++) {
                                    var px = x * bytesPerPixel;
                                    int r = srcRow[px + rOffset];
                                    int g = srcRow[px + gOffset];
                                    int b = srcRow[px + bOffset];
                                    var yVal = Clamp((66 * r + 129 * g + 25 * b + 128) >> 8, 0, 219) + 16;
                                    yDst[dstOffset + x] = (ushort)(yVal << shift);
                                }
                            }
                        }
                        picture.CopyYPlane(new IntPtr(yPtr), yBufferSize);
                    } else {
                        // Compute full YCbCr at 444
                        // Y goes to stride-padded buffer, Cb/Cr go to packed int arrays
                        if (nativePixelSize == 1) {
                            for (var y = 0; y < height; y++) {
                                var srcRow = src.Slice(y * imageStride, width * bytesPerPixel);
                                var yRow = new Span<byte>(yPtr + y * pictureStride, width);
                                var valOffset = y * width;
                                for (var x = 0; x < width; x++) {
                                    var px = x * bytesPerPixel;
                                    int r = srcRow[px + rOffset];
                                    int g = srcRow[px + gOffset];
                                    int b = srcRow[px + bOffset];
                                    var yVal = Clamp((66 * r + 129 * g + 25 * b + 128) >> 8, 0, 219) + 16;
                                    var cbVal = Clamp((-38 * r - 74 * g + 112 * b + 128) >> 8, -112, 112) + 128;
                                    var crVal = Clamp((112 * r - 94 * g - 18 * b + 128) >> 8, -112, 112) + 128;
                                    yRow[x] = (byte)yVal;
                                    cbValues![valOffset + x] = cbVal;
                                    crValues![valOffset + x] = crVal;
                                }
                            }
                        } else {
                            var yDst = new Span<ushort>(yPtr, pictureStride * height);
                            for (var y = 0; y < height; y++) {
                                var srcRow = src.Slice(y * imageStride, width * bytesPerPixel);
                                var yDstOffset = y * pictureStride;
                                var valOffset = y * width;
                                for (var x = 0; x < width; x++) {
                                    var px = x * bytesPerPixel;
                                    int r = srcRow[px + rOffset];
                                    int g = srcRow[px + gOffset];
                                    int b = srcRow[px + bOffset];
                                    var yVal = Clamp((66 * r + 129 * g + 25 * b + 128) >> 8, 0, 219) + 16;
                                    var cbVal = Clamp((-38 * r - 74 * g + 112 * b + 128) >> 8, -112, 112) + 128;
                                    var crVal = Clamp((112 * r - 94 * g - 18 * b + 128) >> 8, -112, 112) + 128;
                                    yDst[yDstOffset + x] = (ushort)(yVal << shift);
                                    cbValues![valOffset + x] = cbVal;
                                    crValues![valOffset + x] = crVal;
                                }
                            }
                        }
                        picture.CopyYPlane(new IntPtr(yPtr), yBufferSize);

                        // Handle chroma planes
                        GetChromaDimensions(chromaFmt, width, height, pictureStride, out var chromaW, out var chromaH, out var chromaStride);
                        var chromaBufferSize = chromaStride * chromaH * nativePixelSize;

                        if (chromaFmt == ChromaFormat.Csp444) {
                            // No subsampling, write packed values to stride-padded buffer
                            var cbBuf = ArrayPool<byte>.Shared.Rent(chromaBufferSize);
                            var crBuf = ArrayPool<byte>.Shared.Rent(chromaBufferSize);
                            try {
                                fixed (byte* cbPtr = cbBuf, crPtr = crBuf) {
                                    WritePackedToStrided(cbValues!, width, height, cbPtr, chromaStride, nativePixelSize, shift);
                                    WritePackedToStrided(crValues!, width, height, crPtr, chromaStride, nativePixelSize, shift);
                                    picture.CopyUPlane(new IntPtr(cbPtr), chromaBufferSize);
                                    picture.CopyVPlane(new IntPtr(crPtr), chromaBufferSize);
                                }
                            } finally {
                                ArrayPool<byte>.Shared.Return(cbBuf);
                                ArrayPool<byte>.Shared.Return(crBuf);
                            }
                        } else {
                            // Subsample and write
                            var cbBuf = ArrayPool<byte>.Shared.Rent(chromaBufferSize);
                            var crBuf = ArrayPool<byte>.Shared.Rent(chromaBufferSize);
                            try {
                                fixed (byte* cbPtr = cbBuf, crPtr = crBuf) {
                                    SubsampleToStrided(cbValues!, width, height, cbPtr, chromaW, chromaH, chromaStride, chromaFmt, nativePixelSize, shift);
                                    SubsampleToStrided(crValues!, width, height, crPtr, chromaW, chromaH, chromaStride, chromaFmt, nativePixelSize, shift);
                                    picture.CopyUPlane(new IntPtr(cbPtr), chromaBufferSize);
                                    picture.CopyVPlane(new IntPtr(crPtr), chromaBufferSize);
                                }
                            } finally {
                                ArrayPool<byte>.Shared.Return(cbBuf);
                                ArrayPool<byte>.Shared.Return(crBuf);
                            }
                        }
                    }
                }
            } finally {
                ArrayPool<byte>.Shared.Return(yBuffer);
                if (cbValues is not null) {
                    ArrayPool<int>.Shared.Return(cbValues);
                }
                if (crValues is not null) {
                    ArrayPool<int>.Shared.Return(crValues);
                }
            }
        }

        private static unsafe void WritePackedToStrided(int[] packed, int width, int height, byte* buffer, int stride, int nativePixelSize, int shift) {
            if (nativePixelSize == 1) {
                for (var y = 0; y < height; y++) {
                    var srcOffset = y * width;
                    var dstOffset = y * stride;
                    for (var x = 0; x < width; x++) {
                        buffer[dstOffset + x] = (byte)packed[srcOffset + x];
                    }
                }
            } else {
                var dst = (ushort*)buffer;
                for (var y = 0; y < height; y++) {
                    var srcOffset = y * width;
                    var dstOffset = y * stride;
                    for (var x = 0; x < width; x++) {
                        dst[dstOffset + x] = (ushort)(packed[srcOffset + x] << shift);
                    }
                }
            }
        }

        private static unsafe void SubsampleToStrided(int[] src, int srcW, int srcH, byte* buffer, int dstW, int dstH, int dstStride, ChromaFormat chromaFmt, int nativePixelSize, int shift) {
            if (chromaFmt == ChromaFormat.Csp420) {
                // 2x2 block average
                if (nativePixelSize == 1) {
                    for (var cy = 0; cy < dstH; cy++) {
                        var sy = cy * 2;
                        var sy1 = Math.Min(sy + 1, srcH - 1);
                        var dstOffset = cy * dstStride;
                        for (var cx = 0; cx < dstW; cx++) {
                            var sx = cx * 2;
                            var sx1 = Math.Min(sx + 1, srcW - 1);
                            var sum = src[sy * srcW + sx] + src[sy * srcW + sx1] + src[sy1 * srcW + sx] + src[sy1 * srcW + sx1];
                            buffer[dstOffset + cx] = (byte)((sum + 2) / 4);
                        }
                    }
                } else {
                    var dst = (ushort*)buffer;
                    for (var cy = 0; cy < dstH; cy++) {
                        var sy = cy * 2;
                        var sy1 = Math.Min(sy + 1, srcH - 1);
                        var dstOffset = cy * dstStride;
                        for (var cx = 0; cx < dstW; cx++) {
                            var sx = cx * 2;
                            var sx1 = Math.Min(sx + 1, srcW - 1);
                            var sum = src[sy * srcW + sx] + src[sy * srcW + sx1] + src[sy1 * srcW + sx] + src[sy1 * srcW + sx1];
                            dst[dstOffset + cx] = (ushort)(((sum + 2) / 4) << shift);
                        }
                    }
                }
            } else if (chromaFmt == ChromaFormat.Csp422) {
                // 2x1 horizontal average
                if (nativePixelSize == 1) {
                    for (var cy = 0; cy < dstH; cy++) {
                        var dstOffset = cy * dstStride;
                        for (var cx = 0; cx < dstW; cx++) {
                            var sx = cx * 2;
                            var sx1 = Math.Min(sx + 1, srcW - 1);
                            var sum = src[cy * srcW + sx] + src[cy * srcW + sx1];
                            buffer[dstOffset + cx] = (byte)((sum + 1) / 2);
                        }
                    }
                } else {
                    var dst = (ushort*)buffer;
                    for (var cy = 0; cy < dstH; cy++) {
                        var dstOffset = cy * dstStride;
                        for (var cx = 0; cx < dstW; cx++) {
                            var sx = cx * 2;
                            var sx1 = Math.Min(sx + 1, srcW - 1);
                            var sum = src[cy * srcW + sx] + src[cy * srcW + sx1];
                            dst[dstOffset + cx] = (ushort)(((sum + 1) / 2) << shift);
                        }
                    }
                }
            }
        }

        private static unsafe void FillNeutralChroma(Picture picture, ChromaFormat chromaFmt, int pictureStride, int height, int bitDepth, int nativePixelSize) {
            GetChromaDimensions(chromaFmt, picture.Width, height, pictureStride, out var chromaW, out var chromaH, out var chromaStride);
            var chromaBufferSize = chromaStride * chromaH * nativePixelSize;
            var buffer = ArrayPool<byte>.Shared.Rent(chromaBufferSize);
            try {
                fixed (byte* bufPtr = buffer) {
                    if (nativePixelSize == 1) {
                        new Span<byte>(bufPtr, chromaStride * chromaH).Fill(128);
                    } else {
                        var neutralValue = (ushort)(128 << Math.Max(bitDepth - 8, 0));
                        new Span<ushort>(bufPtr, chromaStride * chromaH).Fill(neutralValue);
                    }
                    picture.CopyUPlane(new IntPtr(bufPtr), chromaBufferSize);
                    picture.CopyVPlane(new IntPtr(bufPtr), chromaBufferSize);
                }
            } finally {
                ArrayPool<byte>.Shared.Return(buffer);
            }
        }

        private static void GetChromaDimensions(ChromaFormat chromaFmt, int lumaW, int lumaH, int lumaStride, out int chromaW, out int chromaH, out int chromaStride) {
            switch (chromaFmt) {
                case ChromaFormat.Csp420:
                    chromaW = (lumaW + 1) / 2;
                    chromaH = (lumaH + 1) / 2;
                    chromaStride = lumaStride / 2;
                    break;
                case ChromaFormat.Csp422:
                    chromaW = (lumaW + 1) / 2;
                    chromaH = lumaH;
                    chromaStride = lumaStride / 2;
                    break;
                case ChromaFormat.Csp444:
                    chromaW = lumaW;
                    chromaH = lumaH;
                    chromaStride = lumaStride;
                    break;
                default:
                    chromaW = 0;
                    chromaH = 0;
                    chromaStride = 0;
                    break;
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
