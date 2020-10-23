using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace OpenSense.Utilities.DataWriter {
    [Serializable]
    public class DataWriterConfiguration: INotifyPropertyChanged {

        #region INotifyPropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;

        private void SetProperty<T>(ref T field, T value, [CallerMemberName] string propertyName = null) {
            if (!EqualityComparer<T>.Default.Equals(field, value)) {
                field = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            }
        }
        #endregion

        private bool enabled;

        public bool Enabled {
            get => enabled;
            set => SetProperty(ref enabled, value);
        }

        private string filename;

        public string Filename {
            get => filename;
            set => SetProperty(ref filename, value);
        }

        private bool append;

        public bool Append {
            get => append;
            set => SetProperty(ref append, value);
        }

        private bool writeHeaders;

        public bool WriteHeaders {
            get => writeHeaders;
            set => SetProperty(ref writeHeaders, value);
        }

        private bool unixTimeStamp;

        public bool UnixTimeStamp {
            get => unixTimeStamp;
            set => SetProperty(ref unixTimeStamp, value);
        }

    }
}
