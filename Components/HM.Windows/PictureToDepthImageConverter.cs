#nullable enable

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using HMInterop;
using Microsoft.Psi;
using Microsoft.Psi.Imaging;

namespace OpenSense.Components.HM {
    public sealed class PictureToDepthImageConverter : IConsumer<Shared<Picture>>, IProducer<Shared<DepthImage>>, INotifyPropertyChanged, IDisposable {

        #region Ports
        public Receiver<Shared<Picture>> In { get; }

        public Emitter<Shared<DepthImage>> Out { get; }
        #endregion

        #region Options

        #region Input
        private int? inputBitDepth;

        public int? InputBitDepth {
            get => inputBitDepth;
            set => SetProperty(ref inputBitDepth, value);
        }
        #endregion

        #region Depth Metadata
        private DepthValueSemantics depthValueSemantics = DepthValueSemantics.DistanceToPlane;

        public DepthValueSemantics DepthValueSemantics {
            get => depthValueSemantics;
            set => SetProperty(ref depthValueSemantics, value);
        }

        private double depthValueToMetersScaleFactor = 0.001;

        public double DepthValueToMetersScaleFactor {
            get => depthValueToMetersScaleFactor;
            set => SetProperty(ref depthValueToMetersScaleFactor, value);
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

        public PictureToDepthImageConverter(Pipeline pipeline) {
            In = pipeline.CreateReceiver<Shared<Picture>>(this, Process, nameof(In));
            Out = pipeline.CreateEmitter<Shared<DepthImage>>(this, nameof(Out));
        }

        private unsafe void Process(Shared<Picture> picture, Envelope envelope) {
            var picYuv = picture.Resource.PicYuv;
            var actualBitDepth = picture.Resource.Sps.BitDepths.Luma;
            var width = picYuv.Width;
            var height = picYuv.Height;

            // Per-frame validation
            if (InputBitDepth.HasValue && actualBitDepth != InputBitDepth.Value) {
                throw new InvalidOperationException($"InputBitDepth is set to {InputBitDepth.Value} but actual bit depth is {actualBitDepth}.");
            }

            // One-time validation
            if (!validated) {
                validated = true;

                if (BitDepthMappingEnabled) {
                    BitDepthMappingInfo.ValidateParameters(actualBitDepth, targetBits: 16, BitDepthMappingScaleShift, BitDepthMappingInputStart, BitDepthMappingOutputStart);
                } else if (actualBitDepth != 16) {
                    throw new InvalidOperationException($"Source bit depth is {actualBitDepth} but DepthImage requires 16-bit. Enable bit depth mapping to convert.");
                }
            }

            using var depthImage = DepthImagePool.GetOrCreate(width, height, DepthValueSemantics, DepthValueToMetersScaleFactor);
            var imageData = new Span<byte>(depthImage.Resource.ImageData.ToPointer(), depthImage.Resource.Stride * height);
            var imageU16 = MemoryMarshal.Cast<byte, ushort>(imageData);

            if (!BitDepthMappingEnabled) {
                picYuv.WritePlaneToUshorts(ComponentId.Y, imageU16);
            } else {
                var (yPtr, yW, yH, yStride) = picYuv.GetPlaneAccess(ComponentId.Y);
                var yPels = new ReadOnlySpan<int>(yPtr.ToPointer(), yStride * yH);
                BitDepthMapper.MapPlaneToUshorts(yPels, yW, yH, yStride, imageU16, targetBits: 16, BitDepthMappingScaleShift, BitDepthMappingInputStart, BitDepthMappingOutputStart);
            }
            Out.Post(depthImage, envelope.OriginatingTime);
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
