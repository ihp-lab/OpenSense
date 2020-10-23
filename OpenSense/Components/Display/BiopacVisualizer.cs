using Microsoft.Psi;
using Microsoft.Psi.Audio;
using OpenSense.Utilities;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace OpenSense.Components.Display {
    public class BiopacVisualizer : IConsumer<string>, INotifyPropertyChanged {

        public Receiver<string> In { get; private set; }

        public BiopacVisualizer(Pipeline pipeline) {
            In = pipeline.CreateReceiver<string>(this, Process, nameof(In));
            pipeline.PipelineCompleted += PipelineCompleted;

            display.PropertyChanged += (sender, e) => {
                if (e.PropertyName == nameof(display.AudioImage)) {
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Image)));
                }
            };
        }

        private void Process(string data, Envelope envelope) {
            if (currRate == rate) {
                currRate = 0;
                if (strArr.Count() <= 160) {
                    strArr.Add(data);
                } else {
                    display.Update(strArr);
                    strArr.Clear();
                }
            }
            currRate++;
        }

        private void PipelineCompleted(object sender, PipelineCompletedEventArgs e) {
            display.Clear();
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void SetProperty<T>(ref T field, T value, [CallerMemberName] string propertyName = null) {
            if (!EqualityComparer<T>.Default.Equals(field, value)) {
                field = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        private List<string> strArr = new List<string>();
        private int rate = 50;
        private int currRate = 0;

        private DisplayBiopac display = new DisplayBiopac();

        public WriteableBitmap Image {
            get => display.AudioImage;
        }
    }
}
