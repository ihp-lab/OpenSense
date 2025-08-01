using System;

namespace OpenSense.Components.FFMpeg {
    public static class PixelFormatHelper {

        /// <summary>
        /// Check if the pixel format is supported by the FFmpeg interop layer
        /// </summary>
        public static bool IsSupported(this FFMpegInterop.PixelFormat format) => FFMpegInterop.PixelFormatHelper.IsSupported(format);

        /// <summary>
        /// Convert from FFMpeg PixelFormat to Psi PixelFormat.
        /// </summary>
        public static Microsoft.Psi.Imaging.PixelFormat ToPsiPixelFormat(this FFMpegInterop.PixelFormat ffmpegFormat) {
            switch (ffmpegFormat) {
                case FFMpegInterop.PixelFormat.None:
                    return Microsoft.Psi.Imaging.PixelFormat.Undefined;
                case FFMpegInterop.PixelFormat.RGB24:
                    return Microsoft.Psi.Imaging.PixelFormat.RGB_24bpp;
                case FFMpegInterop.PixelFormat.BGR24:
                    return Microsoft.Psi.Imaging.PixelFormat.BGR_24bpp;
                case FFMpegInterop.PixelFormat.BGRA:
                    return Microsoft.Psi.Imaging.PixelFormat.BGRA_32bpp;
                case FFMpegInterop.PixelFormat.Gray16LE:
                    return Microsoft.Psi.Imaging.PixelFormat.Gray_16bpp;
                default:
                    return Microsoft.Psi.Imaging.PixelFormat.Undefined;
            }
        }

        /// <summary>
        /// Convert from Psi PixelFormat to FFMpeg PixelFormat.
        /// </summary>
        public static FFMpegInterop.PixelFormat ToFFMpegPixelFormat(this Microsoft.Psi.Imaging.PixelFormat psiFormat) {
            switch (psiFormat) {
                case Microsoft.Psi.Imaging.PixelFormat.Undefined:
                    return FFMpegInterop.PixelFormat.None;
                case Microsoft.Psi.Imaging.PixelFormat.RGB_24bpp:
                    return FFMpegInterop.PixelFormat.RGB24;
                case Microsoft.Psi.Imaging.PixelFormat.BGR_24bpp:
                    return FFMpegInterop.PixelFormat.BGR24;
                case Microsoft.Psi.Imaging.PixelFormat.BGRA_32bpp:
                    return FFMpegInterop.PixelFormat.BGRA;
                case Microsoft.Psi.Imaging.PixelFormat.Gray_16bpp:
                    if (!BitConverter.IsLittleEndian) {
                        throw new InvalidOperationException("Big endian is not supported.");
                    }
                    return FFMpegInterop.PixelFormat.Gray16LE;
                default:
                    // For unsupported formats, return None (no conversion)
                    return FFMpegInterop.PixelFormat.None;
            }
        }
    }
}
