using Microsoft.Psi;
using Microsoft.Psi.Audio;
using Microsoft.Psi.Components;
using NAudio.CoreAudioApi;
using OpenSense.Utilities;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace OpenSense.Components.Display {
    public class AudioVisualizer : Subpipeline, IConsumer<AudioBuffer>, INotifyPropertyChanged {

        private Connector<AudioBuffer> InConnector;

        public Receiver<AudioBuffer> In => InConnector.In;

        private AudioResampler resampler;

        public AudioVisualizer(Pipeline pipeline) : base(pipeline) {
            InConnector = CreateInputConnectorFrom<AudioBuffer>(pipeline, nameof(In));
            resampler = new AudioResampler(this, new AudioResamplerConfiguration() { OutputFormat = WaveFormat.Create16kHz1ChannelIeeeFloat() });
            InConnector.Out.PipeTo(resampler.In, DeliveryPolicy.Unlimited);
            resampler.Out.Do(Process);
            PipelineCompleted += OnPipelineCompleted;

            display.PropertyChanged += (sender, e) => {
                if (e.PropertyName == nameof(display.AudioImage)) {
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Image)));
                }
            };
        }

        private void Process(AudioBuffer buffer, Envelope envelope) {
            Debug.Assert(buffer.Format.FormatTag == WaveFormatTag.WAVE_FORMAT_IEEE_FLOAT);
            display.Update(buffer);
        }

        private void OnPipelineCompleted(object sender, PipelineCompletedEventArgs e) {
            display.Clear();
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void SetProperty<T>(ref T field, T value, [CallerMemberName] string propertyName = null) {
            if (!EqualityComparer<T>.Default.Equals(field, value)) {
                field = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        private DisplayAudio display = new DisplayAudio();

        public WriteableBitmap Image {
            get => display.AudioImage;
        }
    }
}
