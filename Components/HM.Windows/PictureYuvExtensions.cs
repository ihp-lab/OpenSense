using System;
using System.Buffers;
using System.Numerics;
using HMInterop;
using Microsoft.Psi.Imaging;

namespace OpenSense.Components.HM {
    /// <summary>
    /// Extension methods for PictureYuv Pel buffer operations via GetPlaneAccess.
    /// </summary>
    internal static class PictureYuvExtensions {

        #region Pel to Pel

        /// <summary>
        /// Copy a plane to another PictureYuv at Pel level (no type conversion).
        /// </summary>
        public static unsafe void CopyPlaneTo(this PictureYuv source, ComponentId srcComp, PictureYuv dest, ComponentId dstComp) {
            var (srcPtr, srcW, srcH, srcStride) = source.GetPlaneAccess(srcComp);
            var (dstPtr, dstW, dstH, dstStride) = dest.GetPlaneAccess(dstComp);
            var w = Math.Min(srcW, dstW);
            var h = Math.Min(srcH, dstH);
            var srcPels = new ReadOnlySpan<int>(srcPtr.ToPointer(), srcStride * srcH);
            var dstPels = new Span<int>(dstPtr.ToPointer(), dstStride * dstH);
            for (var y = 0; y < h; y++) {
                srcPels.Slice(y * srcStride, w).CopyTo(dstPels.Slice(y * dstStride, w));
            }
        }

        /// <summary>
        /// Copy a plane to another PictureYuv then apply bit depth mapping in-place on the destination.
        /// </summary>
        public static unsafe void CopyAndMapPlaneTo(this PictureYuv source, ComponentId srcComp, PictureYuv dest, ComponentId dstComp, int targetBits, int scaleShift, int inputStart, int outputStart) {
            source.CopyPlaneTo(srcComp, dest, dstComp);
            var (dstPtr, dstW, dstH, dstStride) = dest.GetPlaneAccess(dstComp);
            var dstPels = new Span<int>(dstPtr.ToPointer(), dstStride * dstH);
            BitDepthMapper.MapPlane(dstPels, dstW, dstH, dstStride, targetBits, scaleShift, inputStart, outputStart);
        }

        #endregion

        #region Pel to External

        /// <summary>
        /// Write a plane to a packed byte buffer, clamping each Pel to [0, 255].
        /// </summary>
        public static unsafe void WritePlaneToBytes(this PictureYuv source, ComponentId componentId, Span<byte> dest) {
            var (ptr, w, h, stride) = source.GetPlaneAccess(componentId);
            var pels = new ReadOnlySpan<int>(ptr.ToPointer(), stride * h);
            var offset = 0;
            for (var y = 0; y < h; y++) {
                var row = pels.Slice(y * stride, w);
                for (var x = 0; x < w; x++) {
                    dest[offset++] = (byte)Math.Clamp(row[x], 0, 255);
                }
            }
        }

        /// <summary>
        /// Write a plane to a packed ushort buffer, truncating each Pel to ushort.
        /// </summary>
        public static unsafe void WritePlaneToUshorts(this PictureYuv source, ComponentId componentId, Span<ushort> dest) {
            var (ptr, w, h, stride) = source.GetPlaneAccess(componentId);
            var pels = new ReadOnlySpan<int>(ptr.ToPointer(), stride * h);
            var offset = 0;
            for (var y = 0; y < h; y++) {
                var row = pels.Slice(y * stride, w);
                for (var x = 0; x < w; x++) {
                    dest[offset++] = (ushort)row[x];
                }
            }
        }

        /// <summary>
        /// Read a single plane as a pooled ushort buffer (strip stride, narrow Pel to ushort).
        /// The caller is responsible for disposing the returned IMemoryOwner to return the buffer to the pool.
        /// Note: the returned Memory may be longer than w * h; use only the first w * h elements.
        /// </summary>
        public static IMemoryOwner<ushort> ReadPlanePacked(this PictureYuv source, ComponentId componentId) {
            var (_, w, h, _) = source.GetPlaneAccess(componentId);
            var owner = MemoryPool<ushort>.Shared.Rent(w * h);
            source.WritePlaneToUshorts(componentId, owner.Memory.Span[..(w * h)]);
            return owner;
        }

        /// <summary>
        /// Read Y/U/V planes as pooled ushort buffers.
        /// The caller is responsible for disposing the returned IMemoryOwner instances.
        /// </summary>
        public static void ReadAllPlanesPacked(this PictureYuv source, out IMemoryOwner<ushort> y, out IMemoryOwner<ushort> u, out IMemoryOwner<ushort> v) {
            y = source.ReadPlanePacked(ComponentId.Y);
            u = source.ReadPlanePacked(ComponentId.Cb);
            v = source.ReadPlanePacked(ComponentId.Cr);
        }

        /// <summary>
        /// Read a single plane as a pooled ushort buffer, applying bit depth mapping.
        /// The caller is responsible for disposing the returned IMemoryOwner.
        /// </summary>
        public static unsafe IMemoryOwner<ushort> ReadPlaneMapped(this PictureYuv source, ComponentId componentId, int targetBitDepth, int scaleShift, int inputStart, int outputStart) {
            var (ptr, w, h, stride) = source.GetPlaneAccess(componentId);
            var pels = new ReadOnlySpan<int>(ptr.ToPointer(), stride * h);
            var pixelCount = w * h;
            var owner = MemoryPool<ushort>.Shared.Rent(pixelCount);
            BitDepthMapper.MapPlaneToUshorts(pels, w, h, stride, owner.Memory.Span[..pixelCount], targetBitDepth, scaleShift, inputStart, outputStart);
            return owner;
        }

        #endregion

        #region External to Pel

        /// <summary>
        /// Read packed ushort data into a plane, widening each ushort to Pel.
        /// </summary>
        public static unsafe void ReadPlaneFromUshorts(this PictureYuv dest, ComponentId componentId, ReadOnlySpan<ushort> src) {
            var (ptr, w, h, stride) = dest.GetPlaneAccess(componentId);
            var pels = new Span<int>(ptr.ToPointer(), stride * h);
            var offset = 0;
            for (var y = 0; y < h; y++) {
                var row = pels.Slice(y * stride, w);
                for (var x = 0; x < w; x++) {
                    row[x] = src[offset++];
                }
            }
        }

        /// <summary>
        /// Read packed ushort data from an IntPtr into a plane, widening each ushort to Pel.
        /// </summary>
        public static unsafe void ReadPlaneFromUshorts(this PictureYuv dest, ComponentId componentId, IntPtr data, int pixelCount) {
            dest.ReadPlaneFromUshorts(componentId, new ReadOnlySpan<ushort>(data.ToPointer(), pixelCount));
        }

        /// <summary>
        /// Copy 8-bit grayscale pixels from an ImageBase into the Y plane (byte → Pel widening).
        /// </summary>
        public static unsafe void ReadYPlaneFromGray8(this PictureYuv dest, ImageBase image) {
            var width = image.Width;
            var height = image.Height;
            var imgStride = image.Stride;
            var src = new ReadOnlySpan<byte>(image.ImageData.ToPointer(), imgStride * height);

            var (yPtr, yW, yH, yStride) = dest.GetPlaneAccess(ComponentId.Y);
            var yPels = new Span<int>(yPtr.ToPointer(), yStride * yH);
            for (var y = 0; y < height; y++) {
                var srcRow = src.Slice(y * imgStride, width);
                var dstRow = yPels.Slice(y * yStride, width);
                for (var x = 0; x < width; x++) {
                    dstRow[x] = srcRow[x];
                }
            }
        }

        /// <summary>
        /// Copy 16-bit grayscale pixels from an ImageBase into the Y plane (ushort → Pel widening, SIMD-optimized).
        /// </summary>
        public static unsafe void ReadYPlaneFromGray16(this PictureYuv dest, ImageBase image) {
            var width = image.Width;
            var height = image.Height;
            var imgStride = image.Stride;

            var (yPtr, yW, yH, yStride) = dest.GetPlaneAccess(ComponentId.Y);
            var yPels = new Span<int>(yPtr.ToPointer(), yStride * yH);

            var vecSize = Vector<ushort>.Count;
            for (var y = 0; y < height; y++) {
                var srcRow = new ReadOnlySpan<ushort>((byte*)image.ImageData.ToPointer() + y * imgStride, width);
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
        }

        #endregion

        #region Fill

        /// <summary>
        /// Fill Cb and Cr planes with neutral chroma value (128 scaled to bit depth).
        /// </summary>
        public static unsafe void FillNeutralChroma(this PictureYuv picture, int bitDepth) {
            var neutralValue = 128 << (bitDepth - 8);
            foreach (var comp in new[] { ComponentId.Cb, ComponentId.Cr }) {
                var (ptr, w, h, stride) = picture.GetPlaneAccess(comp);
                var pels = new Span<int>(ptr.ToPointer(), stride * h);
                for (var y = 0; y < h; y++) {
                    pels.Slice(y * stride, w).Fill(neutralValue);
                }
            }
        }

        #endregion
    }
}
