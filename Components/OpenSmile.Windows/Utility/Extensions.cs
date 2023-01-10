using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace OpenSense.Components.OpenSmile.Utility {
    internal static class Extensions {
        public static Vector<T> CreateVector<T>(this OpenSmileInterop.Vector rawVector, Func<byte[], T> bitConverter) {
            if (rawVector.DataType != typeof(T)) {
                throw new InvalidDataException($"openSMILE data type mismatch: expect {typeof(T).Name} while got {rawVector.DataType.Name}");
            }
            if (bitConverter is null) {
                throw new ArgumentNullException(nameof(bitConverter));
            }
            int dataIdx = 0;
            var fields = new List<Field<T>>();
            foreach (var rawField in rawVector.FieldInfo) {
                var data = rawVector.Data.Skip(dataIdx).Take(rawField.Length);
                var converted = data.Select(arr => bitConverter(arr));
                var field = new Field<T>(rawField.Name, converted);
                fields.Add(field);
                dataIdx += rawField.Length;
            }
            var result = new Vector<T>(rawVector.Time, rawVector.Index, rawVector.LengthSec, fields);
            return result;
        }

        public static Vector<float> CreateVector_Single(this OpenSmileInterop.Vector rawVector) {
            float converter(byte[] arr) => BitConverter.ToSingle(arr, 0);
            return CreateVector(rawVector, converter);
        }

        public static Vector<int> CreateVector_Int32(this OpenSmileInterop.Vector rawVector) {
            int converter(byte[] arr) => BitConverter.ToInt32(arr, 0);
            return CreateVector(rawVector, converter);
        }
    }
}
