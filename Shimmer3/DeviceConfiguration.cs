using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Runtime.CompilerServices;

namespace OpenSense.Components.Shimmer3 {
    public class DeviceConfiguration : INotifyPropertyChanged {

        public static readonly double[] SupportedSampleRates = { 1, 10.2, 51.2, 102.4, 204.8, 256, 512, 1024, };

        public static readonly int[] SupportedBaudRate = { 115200, 1200, 2400, 4800, 9600, 19200, 38400, 57600, 230400, 460800, 921600, };

        private string serialPortName = "COM1";

        public string SerialPortName {
            get => serialPortName;
            set => SetProperty(ref serialPortName, value);
        }

        private int sampleRateIndex = 4;

        [Range(minimum: 0, maximum: 7)]
        public int SampleRateIndex {
            get => sampleRateIndex;
            set => SetProperty(ref sampleRateIndex, value);
        }

        private int baudRateIndex = 9;

        [Range(minimum: 0, maximum: 10)]
        public int BaudRateIndex {
            get => baudRateIndex;
            set => SetProperty(ref baudRateIndex, value);
        }

        private bool internalExpPower = false;

        /// <summary>
        /// Can be used to power PPG sensor.
        /// </summary>
        public bool InternalExpPower {
            get => internalExpPower;
            set => SetProperty(ref internalExpPower, value);
        }

        #region Magic Converters
        public double SampleRate => SupportedSampleRates[SampleRateIndex];

        public int BaudRate => SupportedBaudRate[BaudRateIndex];
        #endregion

        #region INotifyPropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;

        private void SetProperty<T>(ref T field, T value, [CallerMemberName] string propertyName = null) {
            if (!EqualityComparer<T>.Default.Equals(field, value)) {
                field = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            }
        }
        #endregion
    }
}
