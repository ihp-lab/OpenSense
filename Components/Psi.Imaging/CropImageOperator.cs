#nullable enable

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Microsoft.Psi;
using Microsoft.Psi.Calibration;
using Microsoft.Psi.Imaging;

namespace OpenSense.Components.Psi.Imaging;

/// <summary>
/// Crops images to a region, and converts camera intrinsics to match the cropped images.
/// </summary>
public sealed class CropImageOperator : IConsumer<Shared<Image>>, IProducer<Shared<Image>>, INotifyPropertyChanged, IDisposable {

    private readonly Queue<(Shared<Image> Image, DateTime OriginatingTime)> _pendingImages = new();
    private readonly Queue<(ICameraIntrinsics Intrinsics, DateTime OriginatingTime)> _pendingIntrinsics = new();

    #region Ports
    public Receiver<Shared<Image>> In { get; }

    public Receiver<ICameraIntrinsics> IntrinsicsIn { get; }

    public Emitter<Shared<Image>> Out { get; }

    public Emitter<ICameraIntrinsics> IntrinsicsOut { get; }
    #endregion

    #region Options
    private RegionAnchor anchor = RegionAnchor.TopLeft;

    public RegionAnchor Anchor {
        get => anchor;
        set => SetProperty(ref anchor, value);
    }

    private RegionUnit unit = RegionUnit.Pixel;

    public RegionUnit Unit {
        get => unit;
        set => SetProperty(ref unit, value);
    }

    private double x;

    public double X {
        get => x;
        set {
            if (double.IsNaN(value) || double.IsInfinity(value)) {
                throw new ArgumentOutOfRangeException(nameof(X), "X of the crop region must be a number.");
            }
            SetProperty(ref x, value);
        }
    }

    private double y;

    public double Y {
        get => y;
        set {
            if (double.IsNaN(value) || double.IsInfinity(value)) {
                throw new ArgumentOutOfRangeException(nameof(Y), "Y of the crop region must be a number.");
            }
            SetProperty(ref y, value);
        }
    }

    private double width = 640;

    public double Width {
        get => width;
        set {
            if (!(value > 0) || double.IsInfinity(value)) {
                throw new ArgumentOutOfRangeException(nameof(Width), "Width of the crop region must be positive.");
            }
            SetProperty(ref width, value);
        }
    }

    private double height = 480;

    public double Height {
        get => height;
        set {
            if (!(value > 0) || double.IsInfinity(value)) {
                throw new ArgumentOutOfRangeException(nameof(Height), "Height of the crop region must be positive.");
            }
            SetProperty(ref height, value);
        }
    }

    private RegionPadding padding = RegionPadding.Black;

    public RegionPadding Padding {
        get => padding;
        set => SetProperty(ref padding, value);
    }

    private IntrinsicsMatching intrinsicsMatching = IntrinsicsMatching.Latest;

    /// <summary>
    /// Read when the pipeline starts, later changes have no effect.
    /// </summary>
    public IntrinsicsMatching IntrinsicsMatching {
        get => intrinsicsMatching;
        set => SetProperty(ref intrinsicsMatching, value);
    }

    private ImageWithoutIntrinsics imageWithoutIntrinsics = ImageWithoutIntrinsics.Drop;

    public ImageWithoutIntrinsics ImageWithoutIntrinsics {
        get => imageWithoutIntrinsics;
        set => SetProperty(ref imageWithoutIntrinsics, value);
    }

    private IntrinsicsOutput intrinsicsOutput = IntrinsicsOutput.EveryImage;

    public IntrinsicsOutput IntrinsicsOutput {
        get => intrinsicsOutput;
        set => SetProperty(ref intrinsicsOutput, value);
    }
    #endregion

    private IntrinsicsMatching matching;
    private bool intrinsicsConnected;
    private bool intrinsicsEnded;
    private DateTime lastIntrinsicsTime = DateTime.MinValue;
    private ICameraIntrinsics? latestIntrinsics;
    private ICameraIntrinsics? lastOutputIntrinsics;
    private byte[] blackRow = Array.Empty<byte>();
    private byte[] copiedRow = Array.Empty<byte>();

    public CropImageOperator(Pipeline pipeline) {
        In = pipeline.CreateReceiver<Shared<Image>>(this, ProcessFrame, nameof(In));
        IntrinsicsIn = pipeline.CreateReceiver<ICameraIntrinsics>(this, ProcessIntrinsics, nameof(IntrinsicsIn));
        Out = pipeline.CreateEmitter<Shared<Image>>(this, nameof(Out));
        IntrinsicsOut = pipeline.CreateEmitter<ICameraIntrinsics>(this, nameof(IntrinsicsOut));

        IntrinsicsIn.Unsubscribed += OnIntrinsicsUnsubscribed;
        pipeline.PipelineRun += OnPipelineRun;
    }

    #region Pipeline Event Handlers
    private void OnPipelineRun(object? sender, PipelineRunEventArgs args) {
        // Without intrinsics this is plain cropping, images must not wait for intrinsics that never come
        intrinsicsConnected = ((IReceiver)IntrinsicsIn).Source is not null;
        // Images and intrinsics that wait to be matched depend on it
        matching = IntrinsicsMatching;
    }

    private void OnIntrinsicsUnsubscribed(DateTime finalOriginatingTime) {
        intrinsicsEnded = true;
        MatchPendingImages();
    }
    #endregion

    private void ProcessFrame(Shared<Image> frame, Envelope envelope) {
        if (!intrinsicsConnected) {
            PostCropped(frame.Resource, null, envelope.OriginatingTime);
            return;
        }
        switch (matching) {
            case IntrinsicsMatching.Latest:
                if (latestIntrinsics is null) {
                    HandleImageWithoutIntrinsics(envelope.OriginatingTime);
                } else {
                    PostCropped(frame.Resource, latestIntrinsics, envelope.OriginatingTime);
                }
                break;
            case IntrinsicsMatching.Exact:
                // Whether intrinsics exist for this image is only known after later intrinsics have arrived
                _pendingImages.Enqueue((frame.AddRef(), envelope.OriginatingTime));
                MatchPendingImages();
                break;
            default:
                throw new InvalidOperationException();
        }
    }

    private void ProcessIntrinsics(ICameraIntrinsics intrinsics, Envelope envelope) {
        // Kept beyond this call, while the runtime reuses the message object
        var copy = intrinsics.DeepClone();
        switch (matching) {
            case IntrinsicsMatching.Latest:
                latestIntrinsics = copy;
                break;
            case IntrinsicsMatching.Exact:
                _pendingIntrinsics.Enqueue((copy, envelope.OriginatingTime));
                lastIntrinsicsTime = envelope.OriginatingTime;
                MatchPendingImages();
                break;
            default:
                throw new InvalidOperationException();
        }
    }

    private void MatchPendingImages() {
        while (_pendingImages.Count > 0) {
            var (image, originatingTime) = _pendingImages.Peek();
            // Intrinsics older than the oldest waiting image cannot belong to any image anymore
            while (_pendingIntrinsics.Count > 0 && _pendingIntrinsics.Peek().OriginatingTime < originatingTime) {
                _pendingIntrinsics.Dequeue();
            }
            ICameraIntrinsics? intrinsics = null;
            if (_pendingIntrinsics.Count > 0 && _pendingIntrinsics.Peek().OriginatingTime == originatingTime) {
                intrinsics = _pendingIntrinsics.Dequeue().Intrinsics;
            } else if (lastIntrinsicsTime <= originatingTime && !intrinsicsEnded) {
                // Intrinsics of this image may still arrive
                return;
            }
            _pendingImages.Dequeue();
            using (image) {
                if (intrinsics is null) {
                    HandleImageWithoutIntrinsics(originatingTime);
                } else {
                    PostCropped(image.Resource, intrinsics, originatingTime);
                }
            }
        }
    }

    private void HandleImageWithoutIntrinsics(DateTime originatingTime) {
        if (ImageWithoutIntrinsics == ImageWithoutIntrinsics.Error) {
            throw new InvalidOperationException($"There are no camera intrinsics for the image at {originatingTime:O}.");
        }
    }

    private void PostCropped(Image image, ICameraIntrinsics? intrinsics, DateTime originatingTime) {
        var region = GetRegion(image.Width, image.Height);
        CameraIntrinsics? converted = null;
        if (intrinsics is not null) {
            if (intrinsics.ImageWidth != image.Width || intrinsics.ImageHeight != image.Height) {
                throw new InvalidOperationException($"The camera intrinsics are of {intrinsics.ImageWidth}x{intrinsics.ImageHeight} images, but the image is {image.Width}x{image.Height}.");
            }
            // Cropping only moves the origin of pixel coordinates, focal lengths and distortion stay the same
            var transform = intrinsics.Transform.Clone();
            transform[0, 2] -= region.X;
            transform[1, 2] -= region.Y;
            converted = new CameraIntrinsics(region.Width, region.Height, transform, intrinsics.RadialDistortion, intrinsics.TangentialDistortion, intrinsics.ClosedFormDistorts);
        }

        using var result = ImagePool.GetOrCreate(region.Width, region.Height, image.PixelFormat);
        Crop(image, result.Resource, region);
        Out.Post(result, originatingTime);

        if (converted is null) {
            return;
        }
        if (IntrinsicsOutput == IntrinsicsOutput.OnChange && converted.Equals(lastOutputIntrinsics)) {
            return;
        }
        lastOutputIntrinsics = converted;
        IntrinsicsOut.Post(converted, originatingTime);
    }

    /// <summary>
    /// The region in pixels of an image of the given size.
    /// </summary>
    private System.Drawing.Rectangle GetRegion(int imageWidth, int imageHeight) {
        var region = Unit switch {
            RegionUnit.Pixel => ToPixels(Anchor, X, Y, Width, Height),
            RegionUnit.Fraction => ToPixels(Anchor, X * imageWidth, Y * imageHeight, Width * imageWidth, Height * imageHeight),
            _ => throw new NotSupportedException($"Unit {Unit} is not supported."),
        };
        if (region.Width < 1 || region.Height < 1) {
            throw new InvalidOperationException($"The crop region is less than one pixel of the {imageWidth}x{imageHeight} image.");
        }
        if (Padding == RegionPadding.Error && !IsInside(region, imageWidth, imageHeight)) {
            throw new InvalidOperationException($"The crop region (left {region.X}, top {region.Y}, {region.Width}x{region.Height}) is not inside the {imageWidth}x{imageHeight} image.");
        }
        return region;
    }

    /// <summary>
    /// Rounds a region given in pixels to whole pixels.
    /// </summary>
    private static System.Drawing.Rectangle ToPixels(RegionAnchor anchor, double x, double y, double width, double height) {
        var (left, top) = anchor switch {
            RegionAnchor.TopLeft => (x, y),
            RegionAnchor.Center => (x - width / 2, y - height / 2),
            _ => throw new NotSupportedException($"Anchor {anchor} is not supported."),
        };
        // Edges are rounded, not the size, so that a region inside the image stays inside
        return System.Drawing.Rectangle.FromLTRB(Round(left), Round(top), Round(left + width), Round(top + height));
    }

    private static int Round(double value) {
        // Halves always go the same way, so that the size of a region does not depend on its position
        var rounded = Math.Floor(value + 0.5);
        if (rounded < int.MinValue / 2 || rounded > int.MaxValue / 2) {
            throw new InvalidOperationException("The crop region is too far from the image or too large.");
        }
        return (int)rounded;
    }

    private static bool IsInside(System.Drawing.Rectangle region, int imageWidth, int imageHeight) {
        return region.X >= 0 && region.Y >= 0 && (long)region.X + region.Width <= imageWidth && (long)region.Y + region.Height <= imageHeight;
    }

    private void Crop(Image source, Image destination, System.Drawing.Rectangle region) {
        if (IsInside(region, source.Width, source.Height)) {
            source.Crop(destination, region.X, region.Y, region.Width, region.Height);
            return;
        }

        var bytesPerPixel = source.BitsPerPixel / 8;
        var rowLength = region.Width * bytesPerPixel;
        if (blackRow.Length < rowLength) {
            blackRow = new byte[rowLength];
            copiedRow = new byte[rowLength];
        }
        // Pooled images keep their previous content
        for (var row = 0; row < region.Height; row++) {
            Marshal.Copy(blackRow, 0, new IntPtr(destination.ImageData.ToInt64() + (long)row * destination.Stride), rowLength);
        }

        var left = Math.Max((long)region.X, 0);
        var top = Math.Max((long)region.Y, 0);
        var right = Math.Min((long)region.X + region.Width, source.Width);
        var bottom = Math.Min((long)region.Y + region.Height, source.Height);
        if (right <= left || bottom <= top) {
            return;
        }
        var length = (int)(right - left) * bytesPerPixel;
        for (var row = top; row < bottom; row++) {
            Marshal.Copy(new IntPtr(source.ImageData.ToInt64() + row * source.Stride + left * bytesPerPixel), copiedRow, 0, length);
            Marshal.Copy(copiedRow, 0, new IntPtr(destination.ImageData.ToInt64() + (row - region.Y) * destination.Stride + (left - region.X) * bytesPerPixel), length);
        }
    }

    #region IDisposable
    private bool disposed;

    public void Dispose() {
        if (disposed) {
            return;
        }
        disposed = true;

        while (_pendingImages.Count > 0) {
            _pendingImages.Dequeue().Image.Dispose();
        }
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
