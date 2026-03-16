using System;
using System.Buffers;
using HMInterop;

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
        public static unsafe void CopyAndMapPlaneTo(this PictureYuv source, ComponentId srcComp, PictureYuv dest, ComponentId dstComp, int targetBits, int scaleShift, int windowStart) {
            source.CopyPlaneTo(srcComp, dest, dstComp);
            var (dstPtr, dstW, dstH, dstStride) = dest.GetPlaneAccess(dstComp);
            var dstPels = new Span<int>(dstPtr.ToPointer(), dstStride * dstH);
            BitDepthMapper.MapPlane(dstPels, dstW, dstH, dstStride, targetBits, scaleShift, windowStart);
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
