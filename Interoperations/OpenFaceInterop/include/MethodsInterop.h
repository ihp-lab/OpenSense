#pragma once

#pragma managed
#include <msclr\marshal.h>
#include <msclr\marshal_cppstd.h>

#pragma unmanaged

#include <cv.h>
#include <highgui.h>

#include <opencv2/videoio/videoio.hpp>  // Video write
#include <opencv2/videoio/videoio_c.h>  // Video write

#pragma managed

#pragma make_public(cv::Mat)

using namespace System::Windows;
using namespace System::Windows::Threading;
using namespace System::Windows::Media;
using namespace System::Windows::Media::Imaging;

namespace OpenFaceInterop
{
	public ref class RawImage : System::IDisposable
	{

	private:

		cv::Mat* mat;

	public:

		static int PixelFormatToType(PixelFormat fmt)
		{
			if (fmt == PixelFormats::Gray8)
				return CV_8UC1;
			else if (fmt == PixelFormats::Bgr24)
				return CV_8UC3;
			else if (fmt == PixelFormats::Bgra32)
				return CV_8UC4;
			else if (fmt == PixelFormats::Gray32Float)
				return CV_32FC1;
			else
				throw gcnew System::Exception("Unsupported pixel format");
		}

		static PixelFormat TypeToPixelFormat(int type)
		{
			switch (type) {
			case CV_8UC1:
				return PixelFormats::Gray8;
			case CV_8UC3:
				return PixelFormats::Bgr24;
			case CV_8UC4:
				return PixelFormats::Bgra32;
			case CV_32FC1:
				return PixelFormats::Gray32Float;
			default:
				throw gcnew System::Exception("Unsupported image type");
			}
		}

		RawImage(const cv::Mat& m)
		{
			mat = new cv::Mat(m.clone());
		}

		void Mirror()
		{
			cv::flip(*mat, *mat, 1);
		}

		// Finalizer. Definitely called before Garbage Collection,
		// but not automatically called on explicit Dispose().
		// May be called multiple times.
		!RawImage()
		{
			if (mat)
			{
				delete mat;
				mat = NULL;
			}
		}

		// Destructor. Called on explicit Dispose() only.
		~RawImage()
		{
			this->!RawImage();
		}

		property int Width
		{
			int get()
			{
				return mat->cols;
			}
		}

		property int Height
		{
			int get()
			{
				return mat->rows;
			}
		}

		property int Stride
		{

			int get()
			{
				return (int) mat->step;
			}
		}

		property PixelFormat Format
		{
			PixelFormat get()
			{
				return TypeToPixelFormat(mat->type());
			}
		}

		property cv::Mat& Mat
		{
			cv::Mat& get()
			{
				return *mat;
			}
		}

		property bool IsEmpty
		{
			bool get()
			{
				return !mat || mat->empty();
			}
		}

		bool UpdateWriteableBitmap(WriteableBitmap^ bitmap)
		{
			if (bitmap == nullptr || bitmap->PixelWidth != Width || bitmap->PixelHeight != Height || bitmap->Format != Format)
				return false;
			else {
				if (mat->data == NULL) {
					cv::Mat zeros(bitmap->PixelHeight, bitmap->PixelWidth, PixelFormatToType(bitmap->Format), 0);
					bitmap->WritePixels(Int32Rect(0, 0, Width, Height), System::IntPtr(zeros.data), Stride * Height * (Format.BitsPerPixel / 8), Stride, 0, 0);
				}
				else {
					bitmap->WritePixels(Int32Rect(0, 0, Width, Height), System::IntPtr(mat->data), Stride * Height * (Format.BitsPerPixel / 8), Stride, 0, 0);
				}
				return true;
			}
		}

		WriteableBitmap^ CreateWriteableBitmap()
		{
			return gcnew WriteableBitmap(Width, Height, 72, 72, Format, nullptr);
		}

	};

	public ref class VideoWriter
	{
	private:
		// OpenCV based video capture for reading from files
		cv::VideoWriter* vc;

	public:

		VideoWriter(System::String^ location, int width, int height, double fps, bool colour)
		{

			msclr::interop::marshal_context context;
			std::string location_std_string = context.marshal_as<std::string>(location);

			vc = new cv::VideoWriter(location_std_string, CV_FOURCC('D', 'I', 'V', 'X'), fps, cv::Size(width, height), colour);

		}

		// Return success
		bool Write(RawImage^ img)
		{
			if (vc != nullptr && vc->isOpened())
			{
				vc->write(img->Mat);
				return true;
			}
			else
			{
				return false;
			}
		}

		// Finalizer. Definitely called before Garbage Collection,
		// but not automatically called on explicit Dispose().
		// May be called multiple times.
		!VideoWriter()
		{
			if (vc != nullptr)
			{
				vc->~VideoWriter();
			}
		}

		// Destructor. Called on explicit Dispose() only.
		~VideoWriter()
		{
			this->!VideoWriter();
		}

	};

	public ref class ImageBuffer
	{
	public:
		int Width;
		int Height;
		System::IntPtr Data;
		int Stride;

		ImageBuffer(int width, int height, System::IntPtr data, int stride)
		{
			this->Width = width;
			this->Height = height;
			this->Data = data;
			this->Stride = stride;
		}
	};

	public ref class Methods
	{
	public:
		static cv::Mat WrapInMat(ImageBuffer ^img)
		{
			cv::Mat mat = cv::Mat(img->Height, img->Width, CV_MAKETYPE(CV_8U, img->Stride / img->Width), (void *)img->Data, cv::Mat::AUTO_STEP);
			
			return mat;
		}

		static ImageBuffer^ ToGray(ImageBuffer ^color_img, ImageBuffer ^gray_img)
		{
			cv::Mat color_mat = WrapInMat(color_img);
			cv::Mat gray_mat = WrapInMat(gray_img);
			cv::cvtColor(color_mat, gray_mat, cv::COLOR_BGR2GRAY);

			return gray_img;
		}

		static ImageBuffer^ DrawPoint(ImageBuffer ^img, Point p, int r)
		{
			cv::Mat mat = WrapInMat(img);
			cv::circle(mat, cv::Point(p.X, p.Y), r, cv::Scalar(0x66, 0xff, 0x00), -1, 32);

			return img;
		}

		static ImageBuffer^ DrawLine(ImageBuffer ^img, Point p1, Point p2)
		{
			cv::Mat mat = WrapInMat(img);
			cv::line(mat, cv::Point(p1.X, p1.Y), cv::Point(p2.X, p2.Y), cv::Scalar(0x66, 0xff, 0x00), 1, 32);

			return img;
		}

		static ImageBuffer^ FlipVertically(ImageBuffer ^orig_img, ImageBuffer ^vflip_img)
		{
			cv::Mat orig_mat = WrapInMat(orig_img);
			cv::Mat vflip_mat = WrapInMat(vflip_img);

			cv::flip(orig_mat, vflip_mat, 0);

			return vflip_img;
		}

		static ImageBuffer^ FlipHorizontally(ImageBuffer ^orig_img, ImageBuffer ^hflip_img)
		{
			cv::Mat orig_mat = WrapInMat(orig_img);
			cv::Mat hflip_mat = WrapInMat(hflip_img);

			cv::flip(orig_mat, hflip_mat, 1);

			return hflip_img;
		}

		static RawImage^ ToRaw(ImageBuffer ^img)
		{
			cv::Mat mat = WrapInMat(img);
			RawImage^ frame = gcnew RawImage(mat);

			return frame;
		}

		static void SaveImage(ImageBuffer ^img, System::String ^filename)
		{
			cv::Mat mat = WrapInMat(img);
			std::string fn = msclr::interop::marshal_as<std::string>(filename);
			cv::imwrite(fn, mat);
		}
	};
}