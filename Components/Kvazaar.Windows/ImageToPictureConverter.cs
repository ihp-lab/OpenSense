using System;
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

        #region Input
        private PixelFormat? inputPixelFormat;

        public PixelFormat? InputPixelFormat {
            get => inputPixelFormat;
            set => SetProperty(ref inputPixelFormat, value);
        }

        private int? inputBitDepth;

        public int? InputBitDepth {
            get => inputBitDepth;
            set => SetProperty(ref inputBitDepth, value);
        }
        #endregion

        #region Output
#if FIXED_BIT_DEPTH
        public int OutputBitDepth => Picture.MaxBitDepth;
#else
        private int outputBitDepth = 8;

        public int OutputBitDepth {
            get => outputBitDepth;
            set => SetProperty(ref outputBitDepth, value);
        }
#endif

        private ChromaFormat outputChromaFormat = ChromaFormat.Csp400;

        public ChromaFormat OutputChromaFormat {
            get => outputChromaFormat;
            set => SetProperty(ref outputChromaFormat, value);
        }
        #endregion

        #region Chroma Conversion
        private bool chromaConvertEnabled;

        public bool ChromaConvertEnabled {
            get => chromaConvertEnabled;
            set => SetProperty(ref chromaConvertEnabled, value);
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

        private int bitDepthMappingInputStart;

        public int BitDepthMappingInputStart {
            get => bitDepthMappingInputStart;
            set => SetProperty(ref bitDepthMappingInputStart, value);
        }

        private int bitDepthMappingOutputStart;

        public int BitDepthMappingOutputStart {
            get => bitDepthMappingOutputStart;
            set => SetProperty(ref bitDepthMappingOutputStart, value);
        }
        #endregion

        #endregion

        private bool validated;

        public ImageToPictureConverter(Pipeline pipeline) {
            In = pipeline.CreateReceiver<Shared<Image>>(this, Process, nameof(In));
            Out = pipeline.CreateEmitter<Shared<Picture>>(this, nameof(Out));
        }

        private void Process(Shared<Image> image, Envelope envelope) {
            var resource = image.Resource;
            var width = resource.Width;
            var height = resource.Height;

            // Per-frame validation
            if (InputPixelFormat is { } expectedFmt && resource.PixelFormat != expectedFmt) {
                throw new InvalidOperationException($"Expected pixel format {expectedFmt} but got {resource.PixelFormat}.");
            }
            var detectedBits = PixelFormatInfo.GetBitDepth(resource.PixelFormat);
            if (InputBitDepth.HasValue && detectedBits != InputBitDepth.Value) {
                throw new InvalidOperationException($"InputBitDepth is set to {InputBitDepth.Value} but input pixel format {resource.PixelFormat} implies {detectedBits}-bit.");
            }

            var isGray = resource.PixelFormat is PixelFormat.Gray_8bpp or PixelFormat.Gray_16bpp;

            if (!BitDepthMappingEnabled && detectedBits != OutputBitDepth) {
                throw new InvalidOperationException($"Source bit depth is {detectedBits} but output bit depth is {OutputBitDepth}. Enable bit depth mapping to convert.");
            }
            if (!ChromaConvertEnabled) {
                if (isGray && OutputChromaFormat != ChromaFormat.Csp400) {
                    throw new InvalidOperationException($"Output chroma format is {OutputChromaFormat} but input is grayscale. Enable chroma conversion to produce non-monochrome output.");
                }
                if (!isGray && OutputChromaFormat != ChromaFormat.Csp444) {
                    throw new InvalidOperationException($"Output chroma format is {OutputChromaFormat} but color input requires chroma subsampling. Enable chroma conversion.");
                }
            }

            // One-time validation
            if (!validated) {
                validated = true;

                if (OutputBitDepth < 8 || OutputBitDepth > Picture.MaxBitDepth) {
                    throw new InvalidOperationException($"Output bit depth must be between 8 and {Picture.MaxBitDepth}, but was {OutputBitDepth}.");
                }
                if (BitDepthMappingEnabled) {
                    BitDepthMappingInfo.ValidateParameters(detectedBits, OutputBitDepth, BitDepthMappingScaleShift, BitDepthMappingInputStart, BitDepthMappingOutputStart);
                }
            }

            var convertAtOutputBitDepth = BitDepthMappingEnabled && !isGray
                && OutputBitDepth > detectedBits
                && BitDepthMappingInputStart == 0
                && BitDepthMappingOutputStart == 0
                && BitDepthMappingScaleShift == detectedBits - OutputBitDepth;
            var colorConversionBitDepth = convertAtOutputBitDepth ? OutputBitDepth : detectedBits;

            var picture = new Picture(OutputChromaFormat, width, height, OutputBitDepth);

            switch (resource.PixelFormat) {
                case PixelFormat.Gray_8bpp:
                    ColorSpaceConverter.ConvertGray8(resource, picture, OutputChromaFormat);
                    break;
                case PixelFormat.Gray_16bpp:
                    ColorSpaceConverter.ConvertGray16(resource, picture, OutputChromaFormat);
                    break;
                case PixelFormat.BGR_24bpp:
                    ColorSpaceConverter.ConvertColor(resource, picture, OutputChromaFormat, rOffset: 2, gOffset: 1, bOffset: 0, bytesPerPixel: 3, colorConversionBitDepth);
                    break;
                case PixelFormat.RGB_24bpp:
                    ColorSpaceConverter.ConvertColor(resource, picture, OutputChromaFormat, rOffset: 0, gOffset: 1, bOffset: 2, bytesPerPixel: 3, colorConversionBitDepth);
                    break;
                case PixelFormat.BGRA_32bpp:
                case PixelFormat.BGRX_32bpp:
                    ColorSpaceConverter.ConvertColor(resource, picture, OutputChromaFormat, rOffset: 2, gOffset: 1, bOffset: 0, bytesPerPixel: 4, colorConversionBitDepth);
                    break;
                default:
                    picture.Dispose();
                    throw new NotSupportedException($"Pixel format {resource.PixelFormat} is not supported.");
            }

            if (BitDepthMappingEnabled && !convertAtOutputBitDepth) {
                BitDepthMapper.MapAllPlanes(picture, OutputChromaFormat, OutputBitDepth, BitDepthMappingScaleShift, BitDepthMappingInputStart, BitDepthMappingOutputStart);
            }

            using var shared = Shared.Create(picture);
            Out.Post(shared, envelope.OriginatingTime);
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
