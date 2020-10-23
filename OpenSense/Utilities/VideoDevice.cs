using Microsoft.Psi.Media_Interop;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace OpenSense.Utilities {
    public struct Resolution {

        public int Width;

        public int Height;

        public Resolution(int width, int height) {
            Width = width;
            Height = height;
        }

        public override string ToString() => $"{Width} × {Height}";


    }

    public class VideoDevice : INotifyPropertyChanged {
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

        private string symbolicLink;

        public string SymbolicLink {
            get => symbolicLink;
            set => SetProperty(ref symbolicLink, value);
        }

        private ObservableCollection<Resolution> resolutions;

        public ObservableCollection<Resolution> Resolutions {
            get => resolutions;
            set => SetProperty(ref resolutions, value);
        }

        public VideoDevice(int id, string name, string symbolicLink, IEnumerable<Resolution> resolutions) {
            Id = id;
            Name = name;
            SymbolicLink = symbolicLink;
            Resolutions = resolutions is null ? new ObservableCollection<Resolution>() : new ObservableCollection<Resolution>(resolutions);
        }

        public static IEnumerable<VideoDevice> Devices() {
            MediaCaptureDevice.Initialize();
            var id = 0;
            foreach (var device in MediaCaptureDevice.AllDevices) {
                device.Attach(false);
                var resolutions = device.Formats.Where(f => (f.nFrameRateNumerator / f.nFrameRateDenominator) == Properties.Settings.Default.VideoDeviceFrameRate).Select(f => new Resolution(f.nWidth, f.nHeight)).Distinct().ToList();
                if (resolutions.Count > 0) {
                    yield return new VideoDevice(id, device.FriendlyName, device.SymbolicLink, resolutions);
                }
                id++;
            }
        }
    }


}
