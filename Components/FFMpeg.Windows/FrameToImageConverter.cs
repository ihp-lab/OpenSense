using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using FFMpegInterop;
using Microsoft.Psi;
using Microsoft.Psi.Imaging;

namespace OpenSense.Components.FFMpeg;

/// <summary>
/// Converts FFmpeg frames to \psi images.
/// </summary>
public sealed class FrameToImageConverter : IConsumer<Shared<Frame>>, IProducer<Shared<Image>>, INotifyPropertyChanged, IDisposable {

    private readonly FrameConverter _converter = new();

    #region Ports
    public Receiver<Shared<Frame>> In { get; }

    public Emitter<Shared<Image>> Out { get; }
    #endregion

    #region Options
    private Microsoft.Psi.Imaging.PixelFormat outputPixelFormat = Microsoft.Psi.Imaging.PixelFormat.BGR_24bpp;

    public Microsoft.Psi.Imaging.PixelFormat OutputPixelFormat {
        get => outputPixelFormat;
        set => SetProperty(ref outputPixelFormat, value);
    }

    private int outputWidth;

    /// <summary>
    /// Width of output images in pixels. 0 = calculate from OutputHeight and aspect ratio, or original width if both are 0.
    /// </summary>
    public int OutputWidth {
        get => outputWidth;
        set => SetProperty(ref outputWidth, value);
    }

    private int outputHeight;

    /// <summary>
    /// Height of output images in pixels. 0 = calculate from OutputWidth and aspect ratio, or original height if both are 0.
    /// </summary>
    public int OutputHeight {
        get => outputHeight;
        set => SetProperty(ref outputHeight, value);
    }
    #endregion

    private bool validated;

    public FrameToImageConverter(Pipeline pipeline) {
        In = pipeline.CreateReceiver<Shared<Frame>>(this, Process, nameof(In));
        Out = pipeline.CreateEmitter<Shared<Image>>(this, nameof(Out));
    }

    private void Process(Shared<Frame> frame, Envelope envelope) {
        // One-time validation
        if (!validated) {
            validated = true;

            if (OutputPixelFormat.ToFFMpegPixelFormat() == FFMpegInterop.PixelFormat.None) {
                throw new NotSupportedException($"Pixel format {OutputPixelFormat} is not supported.");
            }
            if (OutputWidth < 0) {
                throw new InvalidOperationException($"OutputWidth must be non-negative, but was {OutputWidth}.");
            }
            if (OutputHeight < 0) {
                throw new InvalidOperationException($"OutputHeight must be non-negative, but was {OutputHeight}.");
            }
        }

        var source = frame.Resource;
        var (width, height) = GetOutputSize(source.Width, source.Height);
        using var image = ImagePool.GetOrCreate(width, height, OutputPixelFormat);
        // Row padding of \psi images differs from FFmpeg's
        _converter.Convert(source, OutputPixelFormat.ToFFMpegPixelFormat(), width, height, image.Resource.ImageData, image.Resource.Stride);
        Out.Post(image, envelope.OriginatingTime);
    }

    private (int Width, int Height) GetOutputSize(int sourceWidth, int sourceHeight) {
        if (OutputWidth > 0 && OutputHeight > 0) {
            return (OutputWidth, OutputHeight);
        }
        if (OutputWidth > 0) {
            return (OutputWidth, Math.Max(1, (int)Math.Round((double)sourceHeight * OutputWidth / sourceWidth)));
        }
        if (OutputHeight > 0) {
            return (Math.Max(1, (int)Math.Round((double)sourceWidth * OutputHeight / sourceHeight)), OutputHeight);
        }
        return (sourceWidth, sourceHeight);
    }

    #region IDisposable
    private bool disposed;

    public void Dispose() {
        if (disposed) {
            return;
        }
        disposed = true;

        _converter.Dispose();
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
