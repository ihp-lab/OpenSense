using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using Microsoft.Psi;

namespace OpenSense.Component.CsvExporter {

    public class CsvExporter : Subpipeline, INotifyPropertyChanged {

        

        private readonly string _filename;

        #region Settings
        //None
        #endregion

        private StreamWriter writer;

        private List<StreamSerializer> serializers = new List<StreamSerializer>();

        private bool canAddStream = true;

        private int columnCount = 0;

        private int lastLineColumnCount = 0;

        public CsvExporter(Pipeline pipeline, string filename) : base(pipeline, nameof(CsvExporter)) {
            _filename = filename ?? throw new ArgumentNullException(nameof(filename));
            CreateWriter(append: false);
        }

        #region APIs
        /// <remarks>Call <see cref="FinishInstantiation"/> after all streams are added</remarks>
        public void WriteStream<T>(IProducer<T> source, string streamName, DeliveryPolicy<T> deliveryPolicy = null) {
            if (!canAddStream) {
                throw new InvalidOperationException();
            }
            var serializer = new StreamSerializer<T>(ParentPipeline, streamName, GenerateNewColumnIndex);//TODO: what if "this"?
            serializers.Add(serializer);
            source.PipeTo(serializer, deliveryPolicy);
        }

        /// <summary>
        /// Call after all streams are added.
        /// </summary>
        public void FinishInstantiation() {
            if (!canAddStream) {
                throw new InvalidOperationException();
            }
            var joined = Operators.Join(serializers, Reproducible.Exact<SerializedStreamData>(), DeliveryPolicy.Throttle);
            joined.Do(Process);
            canAddStream = false;
        }
        #endregion

        private void CreateWriter(bool append) {
            var fileStream = File.Open(_filename, append ? FileMode.Append : FileMode.Create, FileAccess.Write, FileShare.Read);
            writer = new StreamWriter(fileStream) {
                NewLine = "\n",
                AutoFlush = false,
            };
        }

        /// <summary>
        /// new columns always append to the end
        /// </summary>
        private int GenerateNewColumnIndex() => Interlocked.Increment(ref columnCount) - 1;

        private static string FormatCvsFieldString(string value) {
            var result = value;
            result = value.Replace("\"", "\"\"");
            if (value.Contains(',')) {
                result = $"\"{result}\"";
            }
            return result;
        }

        private void Process(SerializedStreamData[] streams) {
            if (streams.Length == 0) {
                return;
            }
            var timestamp = streams.First().Envelope.OriginatingTime;
            Debug.Assert(streams.All(s => s.Envelope.OriginatingTime == timestamp));
            var timestampColVal = timestamp.ToString("O");
            var values = new List<string> { timestampColVal };
            

            var visiableColumnsWithValues = serializers
                .Zip(streams, (serializer, data) => (serializer, data))
                .SelectMany(t => t.serializer.Columns.Zip(t.data.ColumnStringValues, (column, columnValue) => (column, columnValue)))
                .Where(t => t.column.Visiable)
                .OrderBy(t => t.column.Index)
                .ToArray();

            var addedColumnCount = visiableColumnsWithValues.Length - lastLineColumnCount;
            if (addedColumnCount > 0) {
                var headers = new List<string> { "utc_time" };
                foreach (var (column, _) in visiableColumnsWithValues) {
                    var formattedHeader = FormatCvsFieldString(column.HeaderName);
                    headers.Add(formattedHeader);
                }
                
                var newHeader = string.Join(",", headers);
                if (lastLineColumnCount == 0) {
                    writer.WriteLine(newHeader);
                } else {
                    writer.Dispose();

                    var tempFilename = Path.GetTempFileName();
                    var tempFileStream = File.OpenWrite(tempFilename);
                    var tempFileWriter = new StreamWriter(tempFileStream) {
                        NewLine = "\n",
                        AutoFlush = false,
                    };
                    tempFileWriter.WriteLine(newHeader);
                    
                    var oldFileStream = File.OpenRead(_filename);
                    var oldFileReader = new StreamReader(oldFileStream);
                    _ = oldFileReader.ReadLine();

                    var commas = new string(',', addedColumnCount);
                    string oldLine;
                    while ((oldLine = oldFileReader.ReadLine()) != null) {
                        tempFileWriter.Write(oldLine);
                        tempFileWriter.WriteLine(commas);//append new columns
                    }

                    oldFileReader.Dispose();//base stream is also closed
                    tempFileWriter.Dispose();//base stream is also closed

                    File.Move(tempFilename, _filename);

                    CreateWriter(append: true);
                }

                lastLineColumnCount = visiableColumnsWithValues.Length;
            }

            foreach (var (_, columnValue) in visiableColumnsWithValues) {
                var formattedValue = FormatCvsFieldString(columnValue);
                values.Add(formattedValue);
            }

            var line = string.Join(",", values);
            writer.WriteLine(line);//TODO: async?
        }

        #region INotifyPropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;

        private void SetProperty<T>(ref T field, T value, [CallerMemberName] string propertyName = null) {
            if (!EqualityComparer<T>.Default.Equals(field, value)) {
                field = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            }
        }
        #endregion

        #region IDisposable
        public override void Dispose() {
            base.Dispose();
            writer?.Dispose();
        }
        #endregion
    }
}
