using System;
using System.Text.Json.Serialization;

namespace OpenSense.Component.Head.Common {
    [Serializable]
    public class ActionUnit: IEquatable<ActionUnit> {

        [JsonInclude]
        public readonly double Intensity;

        [JsonInclude]
        public readonly double Presence;

        [JsonConstructor]
        public ActionUnit(double intensity, double presence) {
            Intensity = intensity;
            Presence = presence;
        }

        #region IEquatable
        public bool Equals(ActionUnit other) =>
            Intensity.Equals(other.Intensity)
            && Presence.Equals(other.Presence)
            ;

        public override bool Equals(object obj) => obj is ActionUnit other ? Equals(obj) : false;

        public override int GetHashCode() => HashCode.Combine(
            Intensity,
            Presence
        );

        public static bool operator ==(ActionUnit a, ActionUnit b) => a.Equals(b);

        public static bool operator !=(ActionUnit a, ActionUnit b) => !(a == b);
        #endregion
    }
}
