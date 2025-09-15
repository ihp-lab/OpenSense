using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Runtime.CompilerServices;
using FFMpegInterop;
using Microsoft.Extensions.Logging;
using Microsoft.Psi;
using Microsoft.Psi.Imaging;

namespace OpenSense.Components.FFMpeg {
    public sealed class FileWriter : INotifyPropertyChanged, IDisposable {

        private readonly FFMpegInterop.FileWriter _writer = new();

        #region Ports
        public Receiver<Shared<Frame>> FrameIn { get; }

        public Receiver<Shared<Image>> ImageIn { get; }
        #endregion

        #region Settings
        private string filename = "video.mp4";

        public string Filename {
            get => filename;
            set => SetProperty(ref filename, value);
        }

        private bool timestampFilename;

        public bool TimestampFilename {
            get => timestampFilename;
            set => SetProperty(ref timestampFilename, value);
        }

        public string Encoder {
            get => _writer.Encoder;
            set {
                if (_writer.Encoder == value) {
                    return;
                }
                _writer.Encoder = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Encoder)));
            }
        }

        public FFMpegInterop.PixelFormat TargetFormat {
            get => _writer.TargetFormat;
            set {
                if (_writer.TargetFormat == value) {
                    return;
                }
                _writer.TargetFormat = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(TargetFormat)));
            }
        }

        public int TargetWidth {
            get => _writer.TargetWidth;
            set {
                if (_writer.TargetWidth == value) {
                    return;
                }
                _writer.TargetWidth = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(TargetWidth)));
            }
        }

        public int TargetHeight {
            get => _writer.TargetHeight;
            set {
                if (_writer.TargetHeight == value) {
                    return;
                }
                _writer.TargetHeight = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(TargetHeight)));
            }
        }

        public string AdditionalArguments {
            get => _writer.AdditionalArguments;
            set {
                if (_writer.AdditionalArguments == value) {
                    return;
                }
                _writer.AdditionalArguments = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(AdditionalArguments)));
            }
        }

        private ILogger? logger;

        public ILogger? Logger {
            get => logger;
            set => SetProperty(ref logger, value);
        }
        #endregion

        public string ActualFilename {
            get => _writer.Filename;
            private set {
                if (_writer.Filename == value) {
                    return;
                }
                _writer.Filename = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ActualFilename)));
            }
        }

        private DateTime? startTime;

        public FileWriter(Pipeline pipeline) {
            FrameIn = pipeline.CreateReceiver<Shared<Frame>>(this, ProcessFrame, nameof(FrameIn));
            ImageIn = pipeline.CreateReceiver<Shared<Image>>(this, ProcessImage, nameof(ImageIn));

            pipeline.PipelineRun += OnPipelineRun;
            pipeline.PipelineCompleted += OnPipelineCompleted;
        }

        #region Pipeline Event Handlers
        private void OnPipelineRun(object? sender, PipelineRunEventArgs args) {
        }

        private void OnPipelineCompleted(object? sender, PipelineCompletedEventArgs args) {
            //TODO: close but not dispose
        }
        #endregion

        private void ProcessFrame(Shared<Frame> frame, Envelope envelope) {
            SaveStartTimeIfFirstFrame(envelope.OriginatingTime);
            var pts = (envelope.OriginatingTime - (DateTime)startTime).Ticks;
            using var clone = new Frame(pts, frame.Resource);
            _writer.WriteFrame(clone);
        }

        private void ProcessImage<TImage>(Shared<TImage> image, Envelope envelope) where TImage : ImageBase {
            SaveStartTimeIfFirstFrame(envelope.OriginatingTime);
            var pts = (envelope.OriginatingTime - (DateTime)startTime).Ticks;
            using var frame = CreateFrame(pts, image.Resource);
            _writer.WriteFrame(frame);
        }

        [MemberNotNull(nameof(startTime))]
        private void SaveStartTimeIfFirstFrame(DateTime originatingTime) {
            if (startTime is not null) {
                return;
            }
            Debug.Assert(originatingTime.Kind == DateTimeKind.Utc);
            startTime = originatingTime;

            if (!TimestampFilename) {
                return;
            }
            var directory = Path.GetDirectoryName(Filename);
            var baseFilename = Path.GetFileNameWithoutExtension(Filename);
            var timestamp = originatingTime.ToString("yyyyMMddHHmmssfffffff");
            var extension = Path.GetExtension(Filename);
            var newFilename = $"{baseFilename}_{timestamp}{extension}";
            ActualFilename = Path.Combine(directory ?? string.Empty, newFilename);
        }

        private static Frame CreateFrame(long pts, ImageBase image) {
            var width = image.Width;
            var height = image.Height;
            var format = image.PixelFormat.ToFFMpegPixelFormat();
            var data = image.UnmanagedBuffer.Data;
            var length = image.UnmanagedBuffer.Size;
            var result = new Frame(pts, width, height, format, data, length);
            return result;
        }

        #region IDisposable
        private bool disposed;

        public void Dispose() {
            if (disposed) {
                return;
            }
            disposed = true;

            _writer.Dispose();
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
