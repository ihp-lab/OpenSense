using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Newtonsoft.Json;

namespace OpenSense.Component.Head.Common {
    [Serializable]
    public class Face : IEquatable<Face> {

        public readonly IReadOnlyDictionary<string, ActionUnit> ActionUnits;

        [JsonConstructor]
        public Face(IDictionary<string, ActionUnit> actionUnits) {
            ActionUnits = actionUnits.ToImmutableSortedDictionary();
        }

        public bool Equals(Face other) => ActionUnits.SequenceEqual(other.ActionUnits);
    }
}
