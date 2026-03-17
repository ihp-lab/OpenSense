#nullable enable

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Numerics;
using System.Runtime.CompilerServices;
using HMInterop;
using Microsoft.Psi;
using Microsoft.Psi.Imaging;

namespace OpenSense.Components.HM {
    public sealed class DepthImageToPictureConverter : IConsumer<Shared<DepthImage>>, IProducer<Shared<Picture>>, INotifyPropertyChanged, IDisposable {

        #region Ports
        public Receiver<Shared<DepthImage>> In { get; }

        public Emitter<Shared<Picture>> Out { get; }
        #endregion

        #region Options

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

        #region Output
        private int outputBitDepth = 16;

        public int OutputBitDepth {
            get => outputBitDepth;
            set => SetProperty(ref outputBitDepth, value);
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
        private int pocCounter;

        public DepthImageToPictureConverter(Pipeline pipeline) {
            In = pipeline.CreateReceiver<Shared<DepthImage>>(this, Process, nameof(In));
            Out = pipeline.CreateEmitter<Shared<Picture>>(this, nameof(Out));
        }

        private unsafe void Process(Shared<DepthImage> depthImage, Envelope envelope) {
            var resource = depthImage.Resource;
            var width = resource.Width;
            var height = resource.Height;

            // Per-frame validation
            if (resource.DepthValueSemantics != DepthValueSemantics) {
                throw new InvalidOperationException($"Expected DepthValueSemantics {DepthValueSemantics} but input has {resource.DepthValueSemantics}.");
            }
            if (Math.Abs(resource.DepthValueToMetersScaleFactor - DepthValueToMetersScaleFactor) > 1e-9) {
                throw new InvalidOperationException($"Expected DepthValueToMetersScaleFactor {DepthValueToMetersScaleFactor} but input has {resource.DepthValueToMetersScaleFactor}.");
            }

            // One-time validation
            if (!validated) {
                validated = true;

                var bitDepth = OutputBitDepth;
                if (bitDepth < 8 || bitDepth > 16) {
                    throw new InvalidOperationException($"Output bit depth must be between 8 and 16, but was {bitDepth}.");
                }
                if (!BitDepthMappingEnabled && bitDepth != 16) {
                    throw new InvalidOperationException($"Source is 16-bit but output bit depth is {bitDepth}. Enable bit depth mapping to convert.");
                }
            }

            var outputBits = OutputBitDepth;
            var picture = PictureYuvPool.Rent(ChromaFormat.Chroma400, width, height);

            var (yPtr, yW, yH, yStride) = picture.GetPlaneAccess(ComponentId.Y);
            var yPels = new Span<int>(yPtr.ToPointer(), yStride * yH);
            var imgStride = resource.Stride;

            var vecSize = Vector<ushort>.Count;
            for (var y = 0; y < height; y++) {
                var srcRow = new ReadOnlySpan<ushort>((byte*)resource.ImageData.ToPointer() + y * imgStride, width);
                var dstRow = yPels.Slice(y * yStride, width);
                var x = 0;
                for (; x + vecSize <= width; x += vecSize) {
                    var srcVec = new Vector<ushort>(srcRow.Slice(x, vecSize));
                    Vector.Widen(srcVec, out var lo, out var hi);
                    Vector.AsVectorInt32(lo).CopyTo(dstRow.Slice(x));
                    Vector.AsVectorInt32(hi).CopyTo(dstRow.Slice(x + Vector<int>.Count));
                }
                for (; x < width; x++) {
                    dstRow[x] = srcRow[x];
                }
            }

            if (BitDepthMappingEnabled) {
                BitDepthMapper.MapPlane(yPels, yW, yH, yStride, outputBits, BitDepthMappingScaleShift, BitDepthMappingInputStart, BitDepthMappingOutputStart);
            }

            var sps = new SequenceParameterSet(
                new BitDepths { Luma = outputBits, Chroma = outputBits },
                ChromaFormat.Chroma400, width, height
            );
            var snapshot = new Picture(picture, pocCounter++, sps, PictureYuvOwnership.Pooled);
            using var shared = Shared.Create(snapshot);
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
