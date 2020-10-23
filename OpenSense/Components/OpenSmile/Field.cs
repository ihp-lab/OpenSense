using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace OpenSense.Components.OpenSmile {
    public class Field {
        public string Name { get; private set; }
        public ImmutableList<float> Data { get; private set; }

        internal Field(string name, IEnumerable<float> data) {
            Name = name;
            Data = data.ToImmutableList();
        }

        public override string ToString() {
            return JsonConvert.SerializeObject(this);
        }

        public override bool Equals(object obj) {
            switch (obj) {
                case Field o:
                    return Name == o.Name && Data.SequenceEqual(o.Data);
                default:
                    return false;
            }
        }
    }
}
