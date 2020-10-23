using Microsoft.Psi;
using Microsoft.Psi.Imaging;
using OpenPose;
using OpenPosePInvoke.Configuration;
using OpenPosePInvoke.Session;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace OpenSense.Components {
    /// <summary>
    /// for demo purpose
    /// </summary>
    public class OpenPoseCountPeopleComponent : IConsumer<OpenPoseDatum>, INotifyPropertyChanged {

        public Receiver<OpenPoseDatum> In { get; private set; }

        public OpenPoseCountPeopleComponent(Pipeline pipeline) {
            In = pipeline.CreateReceiver<OpenPoseDatum>(this, PorcessDatum, nameof(In));
            pipeline.PipelineCompleted += (sender, e) => NumPeople = 0;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void SetProperty<T>(ref T field, T value, [CallerMemberName] string propertyName = null) {
            if (!EqualityComparer<T>.Default.Equals(field, value)) {
                field = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        private int numPeople = 0;

        public int NumPeople {
            get => numPeople;
            set => SetProperty(ref numPeople, value);
        }

        private void PorcessDatum(OpenPoseDatum datum, Envelope envelope) {
            NumPeople = datum.poseKeypoints?.GetSize(0) ?? 0;
        }
    }
}
