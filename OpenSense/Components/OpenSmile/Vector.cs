using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace OpenSense.Components.OpenSmile {
    public class Vector {//readonly fields can not bind to controls

        public double Time { get; private set; }
        public long Index { get; private set; }
        public double LengthSec { get; private set; }
        public Type DataType { get; private set; }
        public ImmutableList<Field> Fields { get; private set; }

        internal Vector(OpenSmileInterop.Vector rawVector) {
            if (rawVector.DataType != typeof(float)) {
                throw new InvalidDataException($"openSMILE data type mismatch: expect {typeof(float).Name} while got {rawVector.DataType.Name}");
            }
            Time = rawVector.Time;
            Index = rawVector.Index;
            LengthSec = rawVector.LengthSec;
            DataType = rawVector.DataType;

            int dataIdx = 0;
            var fields = new List<Field>();
            foreach (var field in rawVector.FieldInfo) {
                fields.Add(new Field(field.Name, rawVector.Data.Skip(dataIdx).Take(field.Length).Select(arr => BitConverter.ToSingle(arr, 0))));
                dataIdx += field.Length;
            }
            Fields = fields.ToImmutableList();
        }

        public override string ToString() {
            return JsonConvert.SerializeObject(this);
        }

        public override bool Equals(object obj) {
            switch (obj) {
                case Vector o:
                    return Time == o.Time && Index == o.Index && LengthSec == o.LengthSec && DataType == o.DataType && Fields.SequenceEqual(o.Fields);
                default:
                    return false;
            }
        }
    }
}
