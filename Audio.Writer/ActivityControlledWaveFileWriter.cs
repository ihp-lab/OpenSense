using System;
using System.IO;
using Microsoft.Psi;
using Microsoft.Psi.Audio;

namespace OpenSense.Component.Audio.Writer {
    /// <summary>
    /// This component will write wave file only when the activity indicator is on.
    /// </summary>
    public class ActivityControlledWaveFileWriter : IConsumer<(AudioBuffer, bool)>, IDisposable {

        private readonly string _filename;

        private WaveDataWriterClass writer;

        public Receiver<(AudioBuffer, bool)> In { get; }

        public ActivityControlledWaveFileWriter(Pipeline pipeline, string filename) {
            In = pipeline.CreateReceiver<(AudioBuffer, bool)>(this, Process, nameof(In));
            _filename = filename ?? throw new ArgumentNullException(nameof(filename));
        }

        private void Process((AudioBuffer, bool) data, Envelope envelope) {
            var (frame, activity) = data;
            if (!activity) {
                return;
            }
            if (writer == null) {
                var filestream = new FileStream(_filename, FileMode.Create);
                var format = frame.Format.DeepClone();
                writer = new WaveDataWriterClass(filestream, format);
            }
            writer.Write(frame.Data);
        }

        public void Dispose() {
            if (writer != null) {
                writer.Dispose();
                writer = null;
            }
        }
    }
}
