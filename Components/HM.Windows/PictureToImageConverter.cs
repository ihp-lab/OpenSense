using System;
using System.Buffers;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using HMInterop;
using Microsoft.Psi;
using Microsoft.Psi.Imaging;

namespace OpenSense.Components.HM {
    public sealed class PictureToImageConverter : IConsumer<Shared<Picture>>, IProducer<Shared<Image>>, INotifyPropertyChanged, IDisposable {

        #region Ports
        public Receiver<Shared<Picture>> In { get; }

        public Emitter<Shared<Image>> Out { get; }
        #endregion

        #region Options

        #region Input
        private int? inputBitDepth;

        public int? InputBitDepth {
            get => inputBitDepth;
            set => SetProperty(ref inputBitDepth, value);
        }
        #endregion

        #region Output
        private PixelFormat outputPixelFormat = PixelFormat.Gray_16bpp;

        public PixelFormat OutputPixelFormat {
            get => outputPixelFormat;
            set => SetProperty(ref outputPixelFormat, value);
        }
        #endregion

        #region Chroma Conversion
        private bool chromaConvertEnabled;

        public bool ChromaConvertEnabled {
            get => chromaConvertEnabled;
            set => SetProperty(ref chromaConvertEnabled, value);
        }

        private ChromaUpsampleMethod chromaUpsampleMethod = ChromaUpsampleMethod.NearestNeighbor;

        public ChromaUpsampleMethod ChromaUpsampleMethod {
            get => chromaUpsampleMethod;
            set => SetProperty(ref chromaUpsampleMethod, value);
        }
        #endregion

        #region Bit Depth Mapping
        private bool bitDepthMappingEnabled;

        public bool BitDepthMappingEnabled {
            get => bitDepthMappingEnabled;
            set => SetProperty(ref bitDepthMappingEnabled, value);
        }

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

        #endregion

        private bool validated;

        public PictureToImageConverter(Pipeline pipeline) {
            In = pipeline.CreateReceiver<Shared<Picture>>(this, Process, nameof(In));
            Out = pipeline.CreateEmitter<Shared<Image>>(this, nameof(Out));
        }

        private int GetEffectiveBitDepth(int actualBitDepth) {
            return BitDepthMappingEnabled ? PixelFormatInfo.GetBitDepth(OutputPixelFormat) : actualBitDepth;
        }

        private void ValidateFirstFrame(PictureYuv picYuv, int actualBitDepth) {
            if (InputBitDepth.HasValue && actualBitDepth != InputBitDepth.Value) {
                throw new InvalidOperationException($"InputBitDepth is set to {InputBitDepth.Value} but actual bit depth is {actualBitDepth}.");
            }

            var targetBitDepth = PixelFormatInfo.GetBitDepth(OutputPixelFormat);
            if (BitDepthMappingEnabled) {
                BitDepthMappingInfo.ValidateParameters(actualBitDepth, targetBitDepth, BitDepthMappingScaleShift, BitDepthMappingWindow);
            } else if (actualBitDepth != targetBitDepth) {
                throw new InvalidOperationException($"Source bit depth is {actualBitDepth} but output pixel format {OutputPixelFormat} requires {targetBitDepth}-bit. Enable bit depth mapping to convert.");
            }

            var requiredChroma = PixelFormatInfo.GetRequiredChromaFormat(OutputPixelFormat);
            if (requiredChroma != ChromaFormat.Chroma400) {
                if (ChromaConvertEnabled) {
                    // Conversion will handle it
                } else if (picYuv.ChromaFormat != requiredChroma) {
                    throw new InvalidOperationException($"Output pixel format {OutputPixelFormat} requires {requiredChroma}, but source is {picYuv.ChromaFormat}. Enable chroma conversion.");
                }
            }
        }

        private void Process(Shared<Picture> picture, Envelope envelope) {
            var picYuv = picture.Resource.PicYuv;
            var actualBitDepth = picture.Resource.Sps.BitDepths.Luma;

            if (!validated) {
                validated = true;
                ValidateFirstFrame(picYuv, actualBitDepth);
            }

            var requiredChroma = PixelFormatInfo.GetRequiredChromaFormat(OutputPixelFormat);
            if (!ChromaConvertEnabled || picYuv.ChromaFormat == requiredChroma) {
                OutputToImagePort(picYuv, actualBitDepth, envelope.OriginatingTime);
            } else {
                var converted = ChromaConverter.Convert(picYuv, requiredChroma, ChromaUpsampleMethod, actualBitDepth);
                try {
                    OutputToImagePort(converted, actualBitDepth, envelope.OriginatingTime);
                } finally {
                    converted.Dispose();
                }
            }
        }

        #region Output
        private unsafe void OutputToImagePort(PictureYuv source, int actualBitDepth, DateTime originatingTime) {
            var width = source.Width;
            var height = source.Height;
            var chroma = source.ChromaFormat;
            var bitDepth = GetEffectiveBitDepth(actualBitDepth);
            var pixelFormat = OutputPixelFormat;
            var targetBitDepth = PixelFormatInfo.GetBitDepth(pixelFormat);

            using var image = ImagePool.GetOrCreate(width, height, pixelFormat);
            var imageData = new Span<byte>(image.Resource.ImageData.ToPointer(), image.Resource.UnmanagedBuffer.Size);

            switch (pixelFormat) {
                case PixelFormat.Gray_8bpp: {
                    if (BitDepthMappingEnabled) {
                        var (yPtr, yW, yH, yStride) = source.GetPlaneAccess(ComponentId.Y);
                        var yPels = new ReadOnlySpan<int>(yPtr.ToPointer(), yStride * yH);
                        BitDepthMapper.MapPlaneToBytes(yPels, yW, yH, yStride, imageData, targetBitDepth, BitDepthMappingScaleShift, BitDepthMappingWindow);
                    } else {
                        source.WritePlaneToBytes(ComponentId.Y, imageData);
                    }
                    break;
                }

                case PixelFormat.Gray_16bpp: {
                    var imageU16 = MemoryMarshal.Cast<byte, ushort>(imageData);
                    if (BitDepthMappingEnabled) {
                        var (yPtr, yW, yH, yStride) = source.GetPlaneAccess(ComponentId.Y);
                        var yPels = new ReadOnlySpan<int>(yPtr.ToPointer(), yStride * yH);
                        BitDepthMapper.MapPlaneToUshorts(yPels, yW, yH, yStride, imageU16, targetBitDepth, BitDepthMappingScaleShift, BitDepthMappingWindow);
                    } else {
                        source.WritePlaneToUshorts(ComponentId.Y, imageU16);
                    }
                    break;
                }

                case PixelFormat.BGR_24bpp:
                case PixelFormat.RGB_24bpp: {
                    if (chroma != ChromaFormat.Chroma444) {
                        throw new InvalidOperationException($"Output pixel format {pixelFormat} requires Chroma444, but source is {chroma}. Enable chroma conversion.");
                    }
                    var pixelCount = width * height;
                    using var yOwner = ReadPlane(source, ComponentId.Y, targetBitDepth, pixelCount);
                    using var uOwner = ReadPlane(source, ComponentId.Cb, targetBitDepth, pixelCount);
                    using var vOwner = ReadPlane(source, ComponentId.Cr, targetBitDepth, pixelCount);
                    var ySpan = yOwner.Memory.Span[..pixelCount];
                    var uSpan = uOwner.Memory.Span[..pixelCount];
                    var vSpan = vOwner.Memory.Span[..pixelCount];
                    if (pixelFormat == PixelFormat.BGR_24bpp) {
                        ColorSpaceConverter.YCbCrToBgr(ySpan, uSpan, vSpan, imageData, width, height, bitDepth);
                    } else {
                        ColorSpaceConverter.YCbCrToRgb(ySpan, uSpan, vSpan, imageData, width, height, bitDepth);
                    }
                    break;
                }

                case PixelFormat.RGBA_64bpp: {
                    if (chroma != ChromaFormat.Chroma444) {
                        throw new InvalidOperationException($"Output pixel format {pixelFormat} requires Chroma444, but source is {chroma}. Enable chroma conversion.");
                    }
                    var pixelCount = width * height;
                    using var yOwner = ReadPlane(source, ComponentId.Y, targetBitDepth, pixelCount);
                    using var uOwner = ReadPlane(source, ComponentId.Cb, targetBitDepth, pixelCount);
                    using var vOwner = ReadPlane(source, ComponentId.Cr, targetBitDepth, pixelCount);
                    var ySpan = yOwner.Memory.Span[..pixelCount];
                    var uSpan = uOwner.Memory.Span[..pixelCount];
                    var vSpan = vOwner.Memory.Span[..pixelCount];
                    ColorSpaceConverter.YCbCrToRgba64(ySpan, uSpan, vSpan, imageData, width, height, bitDepth);
                    break;
                }

                default:
                    throw new NotSupportedException($"Output pixel format {pixelFormat} is not supported.");
            }

            Out.Post(image, originatingTime);
        }

        /// <summary>
        /// Read a plane as a pooled ushort buffer, applying bit depth mapping if enabled.
        /// The caller is responsible for disposing the returned IMemoryOwner.
        /// </summary>
        private unsafe IMemoryOwner<ushort> ReadPlane(PictureYuv source, ComponentId componentId, int targetBitDepth, int pixelCount) {
            if (!BitDepthMappingEnabled) {
                return source.ReadPlanePacked(componentId);
            }
            var (ptr, w, h, stride) = source.GetPlaneAccess(componentId);
            var pels = new ReadOnlySpan<int>(ptr.ToPointer(), stride * h);
            var owner = MemoryPool<ushort>.Shared.Rent(pixelCount);
            BitDepthMapper.MapPlaneToUshorts(pels, w, h, stride, owner.Memory.Span[..pixelCount], targetBitDepth, BitDepthMappingScaleShift, BitDepthMappingWindow);
            return owner;
        }
        #endregion

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
