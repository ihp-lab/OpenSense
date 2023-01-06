using System;
using System.Globalization;
using System.Windows.Data;

namespace OpenSense.WPF.Component.Converters {
    public sealed class IntAbsoluteValueConverter : IValueConverter {

        public string Format { get; set; }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            int integer;
            switch (value) {
                case int i:
                    integer = i;
                    break;
                case string str:
                    integer = int.Parse(str);
                    break;
                default:
                    throw new InvalidOperationException();
            }

            var abs = Math.Abs(integer);

            if (targetType == typeof(int)) {
                return abs;
            } else if (targetType == typeof(string)) {
                return abs.ToString(Format);
            } else {
                throw new InvalidOperationException();
            }            
        }


        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            var result = Convert(value, targetType, parameter, culture);
            return result;
        }
    }
}
