using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace OpenSense.Components.OpenSmile {
    [Serializable]
    public class Field<T> {
        public string Name { get; private set; }

        public IImmutableList<T> Data { get; private set; }

        public Field(string name, IEnumerable<T> data) {
            Name = name;
            Data = data.ToImmutableArray();
        }

        public override bool Equals(object obj) => obj is Field<T> o && Name == o.Name && Data.SequenceEqual(o.Data);

        public override int GetHashCode() => HashCode.Combine(Name, HashCode.Combine(Data));
    }
}
