using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace OpenSense.Components.OpenSmile {
    [Serializable]
	public class Vector<T> {
        /// <summary>
        /// real start time in seconds
        /// </summary>
        public double Time { get; private set; }

        /// <summary>
        /// index of this frame in data memory level
        /// </summary>
        public long Index { get; private set; }

        /// <summary>
        /// real length in seconds
        /// </summary>
        public double LengthSec { get; private set; }

        public IImmutableList<Field<T>> Fields { get; private set; } = Array.Empty<Field<T>>().ToImmutableArray();

        public Vector(double time, long index, double lengthSec, IEnumerable<Field<T>> fields) {
            Time = time;
            Index = index;
            LengthSec = lengthSec;
            Fields = fields.ToImmutableArray();
        }

        public override bool Equals(object obj) =>
            obj is Vector<T> o
            && Time == o.Time
            && Index == o.Index
            && LengthSec == o.LengthSec
            && Fields.SequenceEqual(o.Fields);

        public override int GetHashCode() => HashCode.Combine(Time, Index, LengthSec, HashCode.Combine(Fields));
    }
}
