using Microsoft.Psi;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Psi.Imaging;
using System.Runtime.CompilerServices;
using OpenSense.Utilities;
using System.Windows.Media.Imaging;

namespace OpenSense.Components.Display {
    public class ColorVideoVisualizer : IConsumer<Shared<Image>>, INotifyPropertyChanged {

        #region INotifyPropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;

        private void SetProperty<T>(ref T field, T value, [CallerMemberName] string propertyName = null) {
            if (!EqualityComparer<T>.Default.Equals(field, value)) {
                field = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            }
        }
        #endregion

        public Receiver<Shared<Image>> In { get; private set; }

        public ColorVideoVisualizer(Pipeline pipeline) {
            In = pipeline.CreateReceiver<Shared<Image>>(this, Process, nameof(In));
            pipeline.PipelineCompleted += PipelineCompleted;

            display.PropertyChanged += (sender, e) => {
                if (e.PropertyName == nameof(display.VideoImage)) {
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Image)));
                }
            };
            display.ReceivedFrames.PropertyChanged += (sender, e) => {
                if (e.PropertyName == nameof(display.RenderedFrames.Rate)) {
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(FrameRate)));
                }
            };
        }

        private void Process(Shared<Image> frame, Envelope envelope) {
            display.Update(frame);
        }

        private void PipelineCompleted(object sender, PipelineCompletedEventArgs e) {
            display.Clear();
        }
        private DisplayVideo display = new DisplayVideo();

        public WriteableBitmap Image {
            get => display.VideoImage;
        }

        public int FrameRate {
            get => display.ReceivedFrames.Rate;
        }
    }
}
