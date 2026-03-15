using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using HMInterop;
using Microsoft.Psi;
using Microsoft.Psi.Imaging;

namespace OpenSense.Components.HM {
    public sealed class PictureToImageConverter : IConsumer<Shared<PictureSnapshot>>, IProducer<Shared<Image>>, INotifyPropertyChanged, IDisposable {

        #region Ports
        public Receiver<Shared<PictureSnapshot>> In { get; }

        public Emitter<Shared<Image>> Out { get; }
        #endregion

        #region Options

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

        private int sourceBitDepth;

        public int SourceBitDepth {
            get => sourceBitDepth;
            set => SetProperty(ref sourceBitDepth, value);
        }
        #endregion

        #endregion

        private bool validated;

        public PictureToImageConverter(Pipeline pipeline) {
            In = pipeline.CreateReceiver<Shared<PictureSnapshot>>(this, Process, nameof(In));
            Out = pipeline.CreateEmitter<Shared<Image>>(this, nameof(Out));
        }

        private int GetEffectiveBitDepth(int actualBitDepth) {
            return BitDepthMappingEnabled ? PixelFormatInfo.GetBitDepth(OutputPixelFormat) : actualBitDepth;
        }

        private void ValidateFirstFrame(PictureYuv picYuv, int actualBitDepth) {
            if (SourceBitDepth > 0 && actualBitDepth != SourceBitDepth) {
                throw new InvalidOperationException($"SourceBitDepth is set to {SourceBitDepth} but actual bit depth is {actualBitDepth}.");
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

        private void Process(Shared<PictureSnapshot> picture, Envelope envelope) {
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
                    ReadAndMapAllPlanes(source, targetBitDepth, out var yPacked, out var uPacked, out var vPacked);
                    if (pixelFormat == PixelFormat.BGR_24bpp) {
                        ColorSpaceConverter.YCbCrToBgr(yPacked, uPacked, vPacked, imageData, width, height, bitDepth);
                    } else {
                        ColorSpaceConverter.YCbCrToRgb(yPacked, uPacked, vPacked, imageData, width, height, bitDepth);
                    }
                    break;
                }

                case PixelFormat.RGBA_64bpp: {
                    if (chroma != ChromaFormat.Chroma444) {
                        throw new InvalidOperationException($"Output pixel format {pixelFormat} requires Chroma444, but source is {chroma}. Enable chroma conversion.");
                    }
                    ReadAndMapAllPlanes(source, targetBitDepth, out var yPacked, out var uPacked, out var vPacked);
                    ColorSpaceConverter.YCbCrToRgba64(yPacked, uPacked, vPacked, imageData, width, height, bitDepth);
                    break;
                }

                default:
                    throw new NotSupportedException($"Output pixel format {pixelFormat} is not supported.");
            }

            Out.Post(image, originatingTime);
        }

        /// <summary>
        /// Read all planes as packed ushort, applying bit depth mapping at Pel level if enabled.
        /// </summary>
        private void ReadAndMapAllPlanes(PictureYuv source, int targetBitDepth, out ushort[] y, out ushort[] u, out ushort[] v) {
            if (!BitDepthMappingEnabled) {
                source.ReadAllPlanesPacked(out y, out u, out v);
            } else {
                y = ReadAndMapPlane(source, ComponentId.Y, targetBitDepth);
                u = ReadAndMapPlane(source, ComponentId.Cb, targetBitDepth);
                v = ReadAndMapPlane(source, ComponentId.Cr, targetBitDepth);
            }
        }

        private unsafe ushort[] ReadAndMapPlane(PictureYuv source, ComponentId componentId, int targetBitDepth) {
            var (ptr, w, h, stride) = source.GetPlaneAccess(componentId);
            var pels = new ReadOnlySpan<int>(ptr.ToPointer(), stride * h);
            var result = new ushort[w * h];

            BitDepthMapper.MapPlaneToUshorts(pels, w, h, stride, result, targetBitDepth, BitDepthMappingScaleShift, BitDepthMappingWindow);

            return result;
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
