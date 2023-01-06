#nullable enable

using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Mediapipe.Net.Framework.Packets;

namespace OpenSense.Components.MediaPipe.NET {
    public sealed class InputStreamConfiguration : INotifyPropertyChanged {

        private string identifier = "input_identifier";

        public string Identifier {
            get => identifier;
            set => SetProperty(ref identifier, value);
        }

        private PacketType packetType = PacketType.ImageFrame;

        public PacketType PacketType {
            get => packetType;
            set => SetProperty(ref packetType, value);
        }

        #region INotifyPropertyChanged
        public event PropertyChangedEventHandler? PropertyChanged;

        private void SetProperty<T>(ref T field, T value, [CallerMemberName] string? propertyName = null) {
            if (!EqualityComparer<T>.Default.Equals(field, value)) {
                field = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            }
        }
        #endregion
    }
}
