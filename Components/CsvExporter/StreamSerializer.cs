using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using Microsoft.Psi;

namespace OpenSense.Components.CsvExporter {

    internal abstract class StreamSerializer : IProducer<SerializedStreamData> {

        protected readonly string StreamName;

        protected readonly Func<int> NewColumnIndexGenerator;

        public int MaxRecursionDepth { get; set; } = int.MaxValue;

        public string NullValueResultString { get; set; } = "null";

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

        private CultureInfo Culture => CultureInfo.CurrentCulture;

        public Receiver<T> In { get; }

        public StreamSerializer(Pipeline pipeline, string streamName, Func<int> newColumnIndexGenerator) : base(pipeline, streamName, newColumnIndexGenerator) {
            In = pipeline.CreateReceiver<T>(this, Process, nameof(In));
        }

        private void Process(T data, Envelope e) {
            var columnStringValues = Enumerable.Repeat("", Columns.Count).ToList();//TODO: reuse list for performance
            DecodeRecursively(data, StreamName, columnStringValues, depth: 0);
            var result = new SerializedStreamData() {
                Envelope = e,
                ColumnStringValues = columnStringValues,
            };
            Out.Post(result, e.OriginatingTime);
        }

        private bool TryConvertToString(object data, CultureInfo culture, out string result) {
            if (data is null) {
                result = NullValueResultString ?? "";
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

        private void DecodeRecursively(object data, string headerName, List<string> columnStringValues, int depth) {
            if (depth > MaxRecursionDepth) {
                return;
            }

            //Local
            var column = Columns.Find(c => c.HeaderName == headerName);//TODO: performance
            if (column is null) {
                column = new Column() {
                    Index = NewColumnIndexGenerator(),
                    Visiable = false,
                    HeaderName = headerName,
                };
                Columns.Add(column);
            }
            var shortageCount = Columns.Count - columnStringValues.Count;
            if (shortageCount > 0) {
                columnStringValues.AddRange(Enumerable.Repeat("", shortageCount));
            }
            if (TryConvertToString(data, Culture, out var columnStringValue)) {
                column.Visiable = true;
                var columnIndex = Columns.IndexOf(column);//TODO: performance
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
            foreach (var memberInfo in members) {//TODO: cache this info
                if (memberInfo.IsDefined(typeof(NonSerializedAttribute), inherit: true)) {
                    continue;
                }

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

                DecodeRecursively(member, memberHeaderName, columnStringValues, depth + 1);

                if (member is IEnumerable enumerable) {
                    var index = 0;
                    var newDepth = depth + 1;
                    foreach (var item in enumerable) {
                        var itemType = item.GetType();
                        if (itemType.IsGenericType && itemType.GetGenericTypeDefinition() == typeof(KeyValuePair<,>)) {
                            var key = itemType.GetProperty(nameof(KeyValuePair<int, int>.Key)).GetValue(item);
                            if (TryConvertToString(key, Culture, out var keyStringValue)) {
                                var itemHeaderName = $"{memberHeaderName}[{keyStringValue}]";
                                var val = itemType.GetProperty(nameof(KeyValuePair<int, int>.Value)).GetValue(item);
                                DecodeRecursively(item, itemHeaderName, columnStringValues, newDepth);
                            } else {
                                var genericArugments = itemType.GetGenericArguments();
                                var itemKeyType = genericArugments.First();
                                //var itemValueType = genericArugments.Last();
                                throw new NotSupportedException($"CSV serializer cannot convert type {itemKeyType.Name} as a key");
                            }
                        } else {
                            var itemHeaderName = $"{memberHeaderName}[{index}]";
                            DecodeRecursively(item, itemHeaderName, columnStringValues, newDepth);
                        }
                        index++;
                    }
                }
                
            }
        }

        
    }
}
