namespace OpenSense.Components.FFMpeg;

public static class PixelFormatHelper {

    /// <summary>
    /// Convert from Psi PixelFormat to FFMpeg PixelFormat, None if not supported.
    /// </summary>
    public static FFMpegInterop.PixelFormat ToFFMpegPixelFormat(this Microsoft.Psi.Imaging.PixelFormat psiFormat) {
        return psiFormat switch {
            Microsoft.Psi.Imaging.PixelFormat.Gray_8bpp => FFMpegInterop.PixelFormat.Gray8,
            Microsoft.Psi.Imaging.PixelFormat.Gray_16bpp => FFMpegInterop.PixelFormat.Gray16LE,
            Microsoft.Psi.Imaging.PixelFormat.BGR_24bpp => FFMpegInterop.PixelFormat.BGR24,
            Microsoft.Psi.Imaging.PixelFormat.RGB_24bpp => FFMpegInterop.PixelFormat.RGB24,
            Microsoft.Psi.Imaging.PixelFormat.BGRX_32bpp => FFMpegInterop.PixelFormat.BGR0,
            Microsoft.Psi.Imaging.PixelFormat.BGRA_32bpp => FFMpegInterop.PixelFormat.BGRA,
            Microsoft.Psi.Imaging.PixelFormat.RGBA_64bpp => FFMpegInterop.PixelFormat.RGBA64LE,
            _ => FFMpegInterop.PixelFormat.None,
        };
    }
}
