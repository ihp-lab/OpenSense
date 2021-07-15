using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Reflection;
using Microsoft.Psi;

namespace OpenSense.Component.CsvExporter {

    internal abstract class StreamSerializer : IProducer<SerializedStreamData> {

        protected readonly string StreamName;

        protected readonly Func<int> NewColumnIndexGenerator;

        public List<Column> Columns { get; protected set; } = new List<Column>();

        public Emitter<SerializedStreamData> Out { get; }

        public StreamSerializer(Pipeline pipeline, string streamName, Func<int> newColumnIndexGenerator) {
            Out = pipeline.CreateEmitter<SerializedStreamData>(this, nameof(Out));

            StreamName = streamName ?? throw new ArgumentNullException(nameof(streamName));
            NewColumnIndexGenerator = newColumnIndexGenerator ?? throw new ArgumentNullException(nameof(newColumnIndexGenerator));
        }
    }

    internal sealed class StreamSerializer<T> : StreamSerializer, IConsumer<T> {

        private const BindingFlags Flags = BindingFlags.Public | BindingFlags.Instance | BindingFlags.GetField | BindingFlags.GetProperty;

        private static readonly Type[] NumericTypes = new Type[] { 
            typeof(short), typeof(int), typeof(long), 
            typeof(ushort), typeof(uint), typeof(ulong),
            typeof(nint), typeof(nuint),
            typeof(decimal),
            typeof(float), typeof(double),
        };

        private static readonly Type[] DateTimeTypes = new Type[] {
            typeof(DateTime), typeof(DateTimeOffset),
        };

        private static readonly Type[] DirectConvertTypes = new Type[] { 
            typeof(string), typeof(bool), typeof(byte),
        };

        public Receiver<T> In { get; }

        public StreamSerializer(Pipeline pipeline, string streamName, Func<int> newColumnIndexGenerator) : base(pipeline, streamName, newColumnIndexGenerator) {
            In = pipeline.CreateReceiver<T>(this, Process, nameof(In));
        }

        private void Process(T data, Envelope e) {
            var columnStringValues = Enumerable.Repeat("", Columns.Count).ToList();//TODO: reuse list for performance
            DecodeRecursively(data, StreamName, Columns, columnStringValues, NewColumnIndexGenerator);
            var result = new SerializedStreamData() {
                Envelope = e,
                ColumnStringValues = columnStringValues,
            };
            Out.Post(result, e.OriginatingTime);
        }

        private static bool TryConvertToString(object data, CultureInfo culture, out string result) {
            if (data is null) {
                result = "";
                return true;
            }
            var type = data.GetType();
            if (DirectConvertTypes.Contains(type)) {
                result = data.ToString();
                return true;
            }
            if (NumericTypes.Contains(type)) {
                result = string.Format(culture.NumberFormat, "{0}", data);
                return true;
            }
            if (DateTimeTypes.Contains(type)) {
                result = string.Format(culture.DateTimeFormat, "{0}", data);
                return true;
            }
            /* This method will return type name, not correct
            var converter = TypeDescriptor.GetConverter(type);//TODO: cache it for performance
            if (converter != null && converter.CanConvertTo(typeof(string))) {
                result = converter.ConvertToString(data);
                return true;
            }
            */
            result = null;
            return false;
        }

        private static void DecodeRecursively(object data, string headerName, List<Column> columns, List<string> columnStringValues, Func<int> newColumnIndexGenerator) {
            //Local
            var column = columns.Find(c => c.HeaderName == headerName);//TODO: performance
            if (column is null) {
                column = new Column() {
                    Index = newColumnIndexGenerator(),
                    Visiable = false,
                    HeaderName = headerName,
                };
                columns.Add(column);
            }
            var shortageCount = columns.Count - columnStringValues.Count;
            if (shortageCount > 0) {
                columnStringValues.AddRange(Enumerable.Repeat("", shortageCount));
            }
            if (TryConvertToString(data, CultureInfo.CurrentCulture, out var columnStringValue)) {
                column.Visiable = true;
                var columnIndex = columns.IndexOf(column);//TODO: performance
                columnStringValues[columnIndex] = columnStringValue;
                return;//stop recursion
            }

            //Recursive
            var type = data.GetType();
            var members = type
                .GetFields(Flags)
                .Cast<MemberInfo>()
                .Concat(type.GetProperties(Flags))
                .OrderBy(m => m.Name);
            foreach (var memberInfo in members) {
                var memberHeaderName = $"{headerName}.{memberInfo.Name}";
                object member;
                switch (memberInfo) {
                    case PropertyInfo propInfo:
                        try {
                            member = propInfo.GetValue(data);
                        } catch (TargetParameterCountException) {
                            continue;//indexed property
                        }
                        break;
                    case FieldInfo fieldInfo:
                        member = fieldInfo.GetValue(data);
                        break;
                    default:
                        throw new InvalidOperationException();
                }

                DecodeRecursively(member, memberHeaderName, columns, columnStringValues, newColumnIndexGenerator);

                if (member is IEnumerable enumerable) {
                    var index = 0;
                    foreach (var item in enumerable) {
                        var itemType = item.GetType();
                        if (itemType.IsGenericType && itemType.GetGenericTypeDefinition() == typeof(KeyValuePair<,>)) {
                            var itemKeyType = itemType.GetGenericArguments().First();
                            var itemValueType = itemType.GetGenericArguments().Last();
                            //TODO: dict
                        } else {
                            var itemHeaderName = $"{memberHeaderName}[{index}]";
                            DecodeRecursively(item, itemHeaderName, columns, columnStringValues, newColumnIndexGenerator);
                        }
                        index++;
                    }
                }
                
            }
        }

        
    }
}
