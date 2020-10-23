
namespace OpenSense.Utilities {
    using System;
    using System.Windows;

    using Microsoft.Psi;
    using System.Windows.Media.Imaging;
    using System.Windows.Media;

    using System.Drawing;
    using System.IO;
    using Image = Microsoft.Psi.Imaging.Image;
    using PixelFormat = System.Windows.Media.PixelFormat;
    using System.Drawing.Imaging;

    public static class ImageUtilities {
        public static PixelFormat SharedImageToPixelFormat(Shared<Image> input) {
            System.Windows.Media.PixelFormat pixelFormat;
            WriteableBitmap VideoImage;
            switch (input.Resource.PixelFormat) {
                case Microsoft.Psi.Imaging.PixelFormat.Gray_8bpp:
                    pixelFormat = PixelFormats.Gray8;
                    break;

                case Microsoft.Psi.Imaging.PixelFormat.Gray_16bpp:
                    pixelFormat = PixelFormats.Gray16;
                    break;

                case Microsoft.Psi.Imaging.PixelFormat.BGR_24bpp:
                    pixelFormat = PixelFormats.Bgr24;
                    break;

                case Microsoft.Psi.Imaging.PixelFormat.BGRX_32bpp:
                    pixelFormat = PixelFormats.Bgr32;
                    break;

                case Microsoft.Psi.Imaging.PixelFormat.BGRA_32bpp:
                    pixelFormat = PixelFormats.Bgra32;
                    break;
                default:
                    throw new Exception("Unexpected PixelFormat");
            }
            return pixelFormat;
        }


        public static Bitmap SharedImageToBitmap(Shared<Image> input) {
            PixelFormat pixelFormat = SharedImageToPixelFormat(input);
            WriteableBitmap VideoImage = new WriteableBitmap(input.Resource.Width, input.Resource.Height, 300, 300, pixelFormat, null);
            VideoImage.WritePixels(new Int32Rect(0, 0, input.Resource.Width, input.Resource.Height), input.Resource.ImageData, input.Resource.Stride * input.Resource.Height, input.Resource.Stride);
            Bitmap VideoImageBitmap = BitmapFromWriteableBitmap(VideoImage);
            return VideoImageBitmap;
        }

        private static Bitmap BitmapFromWriteableBitmap(WriteableBitmap writeBmp) {
            System.Drawing.Bitmap bmp;
            using (MemoryStream outStream = new MemoryStream()) {
                BitmapEncoder enc = new BmpBitmapEncoder();
                enc.Frames.Add(BitmapFrame.Create((BitmapSource)writeBmp));
                enc.Save(outStream);
                bmp = new System.Drawing.Bitmap(outStream);
            }
            return bmp;
        }

        public static Bitmap CropBitmap(Bitmap source, int x, int y, int width, int height) {
            Rectangle cropRect = new Rectangle(x, y, width, height);
            Bitmap target = new Bitmap(cropRect.Width, cropRect.Height);
            try {
                using (Graphics g = Graphics.FromImage(target)) {
                    g.DrawImage(source, new Rectangle(0, 0, target.Width, target.Height),
                                     cropRect,
                                     GraphicsUnit.Pixel);
                }
            } catch (Exception e) {
                Console.WriteLine(e.Message);
            }
            return target;
        }

        public static Bitmap AddPadding(Bitmap source, int padding) {
            Rectangle sourceRect = new Rectangle(0, 0, source.Width, source.Height);
            Bitmap target = new Bitmap(source.Width + 2 * padding, source.Height + 2 * padding);
            try {

                using (Graphics g = Graphics.FromImage(target)) {
                    using (SolidBrush brush = new SolidBrush(System.Drawing.Color.FromArgb(122, 122, 122))) {
                        g.FillRectangle(brush, 0, 0, target.Width, target.Height);
                    }
                    g.DrawImage(source, new Rectangle(padding, padding, sourceRect.Width, sourceRect.Height),
                                     sourceRect,
                                     GraphicsUnit.Pixel);
                }
            } catch (Exception e) {
                Console.WriteLine(e.Message);
            }
            return target;
        }

        public static Bitmap ScaleBitmap(Bitmap source, int height, int width) {
            Bitmap resized = new Bitmap(source, new System.Drawing.Size(width, height));
            return resized;
        }

       
    }
}
