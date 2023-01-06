using System;
using System.IO;
using Microsoft.Psi;
using Microsoft.Psi.Audio;

namespace OpenSense.Components.Audio.Writer {
    /// <summary>
    /// This component will write multiple wave files when the activity indicator is on.
    /// </summary>
    public class ActivityControlledWaveFilesWriter : IConsumer<(AudioBuffer, bool)>, IDisposable {

        private readonly Func<Envelope, string> _filenameGenerator;

        private WaveDataWriterClass writer;

        public Receiver<(AudioBuffer, bool)> In { get; }

        protected ActivityControlledWaveFilesWriter(Pipeline pipeline) {
            In = pipeline.CreateReceiver<(AudioBuffer, bool)>(this, Process, nameof(In));
        }

        public ActivityControlledWaveFilesWriter(Pipeline pipeline, Func<Envelope, string> filenameGenerator) : this(pipeline) {
            _filenameGenerator = filenameGenerator ?? throw new ArgumentNullException(nameof(filenameGenerator));
        }

        private void DisposeWriter() {
            if (writer != null) {
                writer.Dispose();
                writer = null; 
            }
        }

        private void Process((AudioBuffer, bool) data, Envelope envelope) {
            var (frame, activity) = data;
            if (activity) {
                if (writer == null) {
                    var filename = _filenameGenerator(envelope);
                    var filestream = new FileStream(filename, FileMode.Create);
                    var format = frame.Format.DeepClone();
                    writer = new WaveDataWriterClass(filestream, format);
                }
                writer.Write(frame.Data);
            } else {
                DisposeWriter();
            }
        }

        public void Dispose() {
            DisposeWriter();
        }
    }
}
