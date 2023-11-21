using System;

namespace OpenSense.Components.MediaPipe.NET {
    /// <summary>
    /// Mediapipe.Net.Framework.Packets.PacketType was removed from MediaPipe.NET, this type is used to maintain compatibility and it should be removed in the future.
    /// </summary>
    public enum PacketType {//TODO: remove and use full qualified name
        Bool,
        Int,
        Float,
        FloatArray,
        String,
        StringAsByteArray,
        ImageFrame,
        Anchor3dVector,
        GpuBuffer,
        ClassificationList,
        ClassificationListVector,
        Detection,
        DetectionVector,
        FaceGeometry,
        FaceGeometryVector,
        FrameAnnotation,
        LandmarkList,
        LandmarkListVector,
        NormalizedLandmarkList,
        NormalizedLandmarkListVector,
        Rect,
        RectVector,
        NormalizedRect,
        NormalizedRectVector,
        TimedModelMatrixProtoList
    }
}
