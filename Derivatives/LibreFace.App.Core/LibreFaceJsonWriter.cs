using System.Reflection;
using System.Text.Json;
using Microsoft.Psi;
using Combind = (
    System.Collections.Generic.IReadOnlyList<System.Collections.Generic.IReadOnlyDictionary<string, bool>> Presence,
    System.Collections.Generic.IReadOnlyList<System.Collections.Generic.IReadOnlyDictionary<string, float>> Intensity,
    System.Collections.Generic.IReadOnlyList<System.Collections.Generic.IReadOnlyDictionary<string, float>> Expression
);

namespace LibreFace.App {
    internal sealed class LibreFaceJsonWriter : IConsumer<Combind> {

        private static readonly JsonWriterOptions Options = new JsonWriterOptions() {
            Indented = false,
        };

        private readonly string _filename;

        private Stream? stream;

        private Utf8JsonWriter? writer;

        private DateTimeOffset? startTime = null;

        #region Ports
        public Receiver<Combind> In { get; }
        #endregion

        public LibreFaceJsonWriter(Pipeline pipeline, string filename) {
            _filename = filename;

            In = pipeline.CreateReceiver<Combind>(this, Process, nameof(In));

            pipeline.PipelineRun += OnPipelineRun;
            pipeline.PipelineCompleted += OnPipelineCompleted;
        }

        private void Process(Combind data, Envelope envelope) {
            var (iP, iI, iE) = data;
            var presences = (List<ActionUnitPresenceOutput>)iP;
            var intensities = (List<ActionUnitIntensityOutput>)iI;
            var expressions = (List<ExpressionOutput>)iE;
            if (presences.Count != intensities.Count || intensities.Count != expressions.Count) {
                throw new InvalidOperationException("Data length mismatch.");
            }
            var count = presences.Count;
            writer!.WriteStartObject();
            var diff = envelope.OriginatingTime - (DateTimeOffset)startTime!;
            var timestamp = diff.TotalMilliseconds;
            writer.WriteNumber("Timestamp", timestamp);

            writer.WriteStartArray("Faces");
            for (var i = 0; i < count; i++) {
                writer.WriteStartObject();


                var presense = presences[i];
                writer.WriteStartObject("Presence");
                foreach (var au in ActionUnitPresenceOutput.Keys) {
                    writer.WriteNumber(au, presense.RawValues[au]);
                }
                writer.WriteEndObject();

                var intensity = intensities[i];
                writer.WriteStartObject("Intensity");
                foreach(var au in ActionUnitIntensityOutput.Keys) {
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

        #region Pipeline Event Handlers
        private void OnPipelineRun(object? sender, PipelineRunEventArgs args) {
            startTime = args.StartOriginatingTime;
            stream = File.OpenWrite(_filename);
            writer = new Utf8JsonWriter(stream, Options);
            writer.WriteStartObject();
            var asm = Assembly.GetAssembly(typeof(Processor));
            var asmName = asm!.GetName();
            var ver = asmName.Version;
            writer.WritePropertyName("Version");
            JsonSerializer.Serialize(writer, ver);
            writer.WriteStartArray("Frames");
        }

        private void OnPipelineCompleted(object? sender, PipelineCompletedEventArgs args) {
            writer!.WriteEndArray();
            writer.WriteEndObject();
            writer.Dispose();
            stream!.Dispose();
        }
        #endregion
    }
}
