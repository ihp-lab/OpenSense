#nullable enable

using System;
using System.Globalization;
using System.Windows.Data;

namespace OpenSense.WPF.Components.ParallelPorts {
    internal sealed class HexShortConverter : IValueConverter {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            return value is short s ? s.ToString("X4") : string.Empty;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            var text = value?.ToString();
            return string.IsNullOrWhiteSpace(text) ? 
                (short)0
                :
                short.TryParse(text.Trim(), NumberStyles.HexNumber, null, out var result) ?
                    result 
                    : 
                    (short)0;
        }
    }
}
