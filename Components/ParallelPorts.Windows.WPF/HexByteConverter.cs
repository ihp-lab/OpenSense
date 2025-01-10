#nullable enable

using System;
using System.Globalization;
using System.Windows.Data;

namespace OpenSense.WPF.Components.ParallelPorts {
    internal sealed class HexByteConverter : IValueConverter {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            return value is byte b ? b.ToString("X2") : string.Empty;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            var text = value?.ToString();
            return string.IsNullOrWhiteSpace(text) ? 
                (byte)0
                :
                byte.TryParse(text.Trim(), NumberStyles.HexNumber, provider: null, out var result) ? 
                    result 
                    :
                    (byte)0;
        }
    }
}
