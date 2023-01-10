using System;
using System.Text.Json.Serialization;

namespace OpenSense.Components.OpenFace {
    public sealed class CameraCalibration : IEquatable<CameraCalibration> {

        [JsonInclude]
        public readonly float FocalLengthX;

        [JsonInclude]
        public readonly float FocalLengthY;

        [JsonInclude]
        public readonly float CenterX;

        [JsonInclude]
        public readonly float CenterY;

        [JsonConstructor]
        public CameraCalibration(float fX, float fY, float cX, float cY) {
            FocalLengthX = fX;
            FocalLengthY = fY;
            CenterX = cX;
            CenterY = cY;
        }

        #region IEquatable
        public bool Equals(CameraCalibration other) =>
            FocalLengthX.Equals(other.FocalLengthX)
            && FocalLengthY.Equals(other.FocalLengthY)
            && CenterX.Equals(other.CenterX)
            && CenterY.Equals(other.CenterY)
            ;

        public override bool Equals(object obj) => obj is CameraCalibration other ? Equals(other) : false;

        public override int GetHashCode() => HashCode.Combine(
            FocalLengthX,
            FocalLengthY,
            CenterX,
            CenterY
        );

        public static bool operator ==(CameraCalibration a, CameraCalibration b) => a.Equals(b);

        public static bool operator !=(CameraCalibration a, CameraCalibration b) => !(a == b);
        #endregion
    }
}
