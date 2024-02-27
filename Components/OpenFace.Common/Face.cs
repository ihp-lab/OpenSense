using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text.Json.Serialization;

namespace OpenSense.Components.OpenFace {
    [Serializable]
    public sealed class Face : IEquatable<Face> {

        public IReadOnlyDictionary<string, ActionUnit> ActionUnits { get; }

        [JsonConstructor]
        internal Face(IReadOnlyDictionary<string, ActionUnit> actionUnits) {
            ActionUnits = actionUnits;
        }

        public Face(IDictionary<string, ActionUnit> actionUnits) {
            ActionUnits = actionUnits.ToImmutableSortedDictionary();
        }

        #region IEquatable
        public bool Equals(Face other) =>
            ActionUnits.SequenceEqual(other.ActionUnits);

        public override bool Equals(object obj) => obj is Face other ? Equals(other) : false;

        public override int GetHashCode() => HashCode.Combine(
            ActionUnits
        );

        public static bool operator ==(Face a, Face b) => a.Equals(b);

        public static bool operator !=(Face a, Face b) => !(a == b);
        #endregion
    }
}
