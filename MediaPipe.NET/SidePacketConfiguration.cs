#nullable enable

using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Mediapipe.Net.Framework.Packets;
using Newtonsoft.Json.Linq;

namespace OpenSense.Component.MediaPipe.NET {
    public sealed class SidePacketConfiguration : INotifyPropertyChanged {

        private string identifier = "side_packet_identifier";

        public string Identifier {
            get => identifier;
            set => SetProperty(ref identifier, value);
        }

        private PacketType packetType = PacketType.Bool;

        public PacketType PacketType {
            get => packetType;
            set => SetProperty(ref packetType, value);
        }

        private JToken? value = false;

        public JToken? Value {
            get => value;
            set => SetProperty(ref this.value, value);
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
