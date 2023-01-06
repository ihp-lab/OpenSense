using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Microsoft.Psi.Audio;

namespace OpenSense.Components.Psi.Audio {
    public class AudioDevice : INotifyPropertyChanged {
        #region INotifyPropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;

        private void SetProperty<T>(ref T field, T value, [CallerMemberName] string propertyName = null) {
            if (!EqualityComparer<T>.Default.Equals(field, value)) {
                field = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            }
        }
        #endregion
        private int id;

        public int Id {
            get => id;
            set => SetProperty(ref id, value);
        }

        private string name;

        public string Name {
            get => name;
            set => SetProperty(ref name, value);
        }

        public AudioDevice(int id, string name) {
            Id = id;
            Name = name;
        }

        public static IEnumerable<AudioDevice> Devices() {
            var id = 0;
            foreach (var device in AudioCapture.GetAvailableDevices()) {
                yield return new AudioDevice(id, device);
                id++;
            }
        }
    }
}
