using System;
using System.Text.Json.Serialization;

namespace OpenSense.Components.OpenFace {
    public struct CameraCalibration : IEquatable<CameraCalibration> {

        [JsonInclude]
        public readonly float CenterX;

        [JsonInclude]
        public readonly float CenterY;

        [JsonInclude]
        public readonly float FocalLengthX;

        [JsonInclude]
        public readonly float FocalLengthY;

        [JsonConstructor]
        public CameraCalibration(float cX, float cY, float fX, float fY) {
            CenterX = cX;
            CenterY = cY;
            FocalLengthX = fX;
            FocalLengthY = fY;
        }

        #region IEquatable
        public bool Equals(CameraCalibration other) =>
            CenterX.Equals(other.CenterX)
            && CenterY.Equals(other.CenterY)
            && FocalLengthX.Equals(other.FocalLengthX)
            && FocalLengthY.Equals(other.FocalLengthY)
            ;

        public override bool Equals(object obj) => obj is CameraCalibration other ? Equals(other) : false;

        public override int GetHashCode() => HashCode.Combine(
            CenterX,
            CenterY,
            FocalLengthX,
            FocalLengthY
        );

        public static bool operator ==(CameraCalibration a, CameraCalibration b) => a.Equals(b);

        public static bool operator !=(CameraCalibration a, CameraCalibration b) => !(a == b);
        #endregion
    }
}
