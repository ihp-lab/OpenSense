#nullable enable

using System.ComponentModel;
using System.IO;
using System.Text;

namespace OpenSense.WPF {
    public sealed class ObservableLogWriter : TextWriter, INotifyPropertyChanged {

        private const int KeepLenght = 10000;

        private static readonly PropertyChangedEventArgs Args = new PropertyChangedEventArgs(nameof(Text));

        private readonly object _bufferLock = new object();

        private readonly object _notificationLock = new object();

        private readonly StringBuilder _buffer = new StringBuilder();

        public string Text => _buffer.ToString();

        public void Clear() {
            lock (_bufferLock) {
                _buffer.Clear();
            }
            lock (_notificationLock) {
                PropertyChanged?.Invoke(this, Args);
            }
        }

        #region INotifyPropertyChanged
        public event PropertyChangedEventHandler? PropertyChanged;
        #endregion

        #region TextWriter
        public override Encoding Encoding => Encoding.UTF8;

        public override void Write(string? value) {
            if (value is null) {
                return;
            }
            lock (_bufferLock) {
                _buffer.Append(value);
                if (_buffer.Length > KeepLenght) {
                    _buffer.Remove(0, _buffer.Length - KeepLenght);
                }
            }
            lock (_notificationLock) {
                PropertyChanged?.Invoke(this, Args);
            }
        }
        #endregion
    }
}
