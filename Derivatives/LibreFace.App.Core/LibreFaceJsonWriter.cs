using System.ComponentModel;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text.Json;
using Mediapipe.Net.Framework.Protobuf;
using Microsoft.Psi;
using Combind = (
    System.Collections.Generic.IReadOnlyList<Mediapipe.Net.Framework.Protobuf.NormalizedLandmarkList> Landmarks,
    System.Collections.Generic.IReadOnlyList<System.Collections.Generic.IReadOnlyDictionary<string, bool>> Presence,
    System.Collections.Generic.IReadOnlyList<System.Collections.Generic.IReadOnlyDictionary<string, float>> Intensity,
    System.Collections.Generic.IReadOnlyList<System.Collections.Generic.IReadOnlyDictionary<string, float>> Expression
);

namespace LibreFace.App {
    internal sealed class LibreFaceJsonWriter : IConsumer<Combind>, INotifyPropertyChanged {

        internal static readonly JsonWriterOptions Options = new JsonWriterOptions() {
            Indented = false,
        };

        private readonly string _filename;

        private Stream? stream;

        private Utf8JsonWriter? writer;

        private DateTimeOffset? startTime = null;

        #region Ports
        public Receiver<Combind> In { get; }
        #endregion

        private long counter;

        public long Counter {
            get => counter;
            private set => SetProperty(ref counter, value);
        }

        public LibreFaceJsonWriter(Pipeline pipeline, string filename) {
            _filename = filename;

            In = pipeline.CreateReceiver<Combind>(this, Process, nameof(In));

            pipeline.PipelineRun += OnPipelineRun;
            pipeline.PipelineCompleted += OnPipelineCompleted;
        }

        private void Process(Combind data, Envelope envelope) {
            var (iL, iP, iI, iE) = data;
            var landmarks = (List<NormalizedLandmarkList>)iL;
            var presences = (List<ActionUnitPresenceOutput>)iP;
            var intensities = (List<ActionUnitIntensityOutput>)iI;
            var expressions = (List<ExpressionOutput>)iE;
            var diff = envelope.OriginatingTime - (DateTimeOffset)startTime!;
            WriteFrame(writer!, Counter, landmarks, presences, intensities, expressions, diff);
            Counter++;
        }

        #region Pipeline Event Handlers
        private void OnPipelineRun(object? sender, PipelineRunEventArgs args) {
            startTime = args.StartOriginatingTime;
            stream = File.OpenWrite(_filename);
            writer = new Utf8JsonWriter(stream, Options);
            WriteStart(writer);
        }

        private void OnPipelineCompleted(object? sender, PipelineCompletedEventArgs args) {
            WriteEnd(writer!);
            writer!.Dispose();
            stream!.Dispose();
        }
        #endregion

        #region Static Methods
        internal static void WriteStart(Utf8JsonWriter writer) {
            writer.WriteStartObject();
            var asm = Assembly.GetAssembly(typeof(LibreFaceJsonWriter));
            var asmName = asm!.GetName();
            var ver = asmName.Version;
            writer.WritePropertyName("Version");
            JsonSerializer.Serialize(writer, ver);
            writer.WriteStartArray("Frames");
        }

        internal static void WriteFrame(Utf8JsonWriter writer, long index, IReadOnlyList<NormalizedLandmarkList>? landmarks, IReadOnlyList<ActionUnitPresenceOutput> presences, IReadOnlyList<ActionUnitIntensityOutput> intensities, IReadOnlyList<ExpressionOutput> expressions, TimeSpan time) {
            landmarks ??= Array.Empty<NormalizedLandmarkList>();
            if (presences.Count != intensities.Count || intensities.Count != expressions.Count) {
                throw new InvalidOperationException("Data length mismatch.");
            }
            var count = presences.Count;
            writer!.WriteStartObject();
            writer.WriteNumber("Index", index);
            var timestamp = time.TotalMilliseconds;
            writer.WriteNumber("Timestamp", timestamp);
            writer.WriteStartArray("Faces");
            for (var i = 0; i < count; i++) {
                writer.WriteStartObject();

                var landmarkList = landmarks[i];
                writer.WriteStartArray("Landmarks");
                for (var j = 0; j < landmarkList.Landmark.Count; j++) {
                    var landmark = landmarkList.Landmark[j];
                    writer.WriteStartObject();
                    writer.WriteNumber("Visibility", landmark.Visibility);
                    writer.WriteNumber("Presence", landmark.Presence);
                    writer.WriteNumber("X", landmark.X);
                    writer.WriteNumber("Y", landmark.Y);
                    writer.WriteNumber("Z", landmark.Z);
                    writer.WriteEndObject();
                }
                writer.WriteEndArray();

                var presense = presences[i];
                writer.WriteStartObject("Presence");
                foreach (var au in ActionUnitPresenceOutput.Keys) {
                    writer.WriteNumber(au, presense.RawValues[au]);
                }
                writer.WriteEndObject();

                var intensity = intensities[i];
                writer.WriteStartObject("Intensity");
                foreach (var au in ActionUnitIntensityOutput.Keys) {
                    writer.WriteNumber(au, intensity[au]);
                }
                writer.WriteEndObject();

                var expression = expressions[i];
                writer.WriteStartObject("Expression");
                foreach (var ex in ExpressionOutput.Keys) {
                    writer.WriteNumber(ex, expression[ex]);
                }
                writer.WriteEndObject();

                writer.WriteEndObject();
            }
            writer.WriteEndArray();

            writer.WriteEndObject();

            writer.Flush();
        }

        internal static void WriteEnd(Utf8JsonWriter writer) {
            writer.WriteEndArray();
            writer.WriteEndObject();
        }
        #endregion

        #region INotifyPropertyChanged
        public event PropertyChangedEventHandler? PropertyChanged;

        private void SetProperty<T>(ref T field, T value, [CallerMemberName] string? propertyName = null) {
            if (!EqualityComparer<T>.Default.Equals(field, value)) {
                field = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            }
        }
        #endregion
    }
}
