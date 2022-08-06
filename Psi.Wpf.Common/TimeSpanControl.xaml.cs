using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using OpenSense.Wpf.Component.Converters;

namespace OpenSense.Wpf.Component.Psi.Common {
    public partial class TimeSpanControl : UserControl
    {
        public static readonly DependencyProperty ValueProperty = DependencyProperty.Register(
            nameof(Value),
            typeof(TimeSpan),
            typeof(TimeSpanControl),
            new PropertyMetadata(defaultValue: TimeSpan.Zero, OnValueChanged)
        );

        public static readonly RoutedEvent ValueChangedEvent = EventManager.RegisterRoutedEvent(
            name: nameof(ValueChanged),
            routingStrategy: RoutingStrategy.Direct,
            handlerType: typeof(RoutedEventHandler),
            ownerType: typeof(TimeSpanControl)
        );

        public TimeSpan Value {
            get => (TimeSpan)GetValue(ValueProperty);
            set {
                if (Value == value) {
                    return;
                }
                SetValue(ValueProperty, value);
            }
        }

        public event RoutedEventHandler ValueChanged {
            add => AddHandler(ValueChangedEvent, value);
            remove => RemoveHandler(ValueChangedEvent, value);
        }

        public TimeSpanControl() {
            InitializeComponent();
        }

        private void OnValueChanged(DependencyPropertyChangedEventArgs args) {
            /** Change Sign radio buttons
             */
            switch(Sign(Value)) {
                case 1:
                    RadioButtonPositive.IsChecked = true;
                    break;
                case -1:
                    RadioButtonNegative.IsChecked = true;
                    break;
                default:
                    throw new InvalidOperationException();
            }

            /** Redo bindings, otherwise the Value in TimeSpanControl will not be updated.
             *  Create a new Binding object and bind it, because GetBindingExpression() will return null here.
             */
            void bind(string propertyName, TextBox textBox, string format = null) {
                var binding = new Binding($"{nameof(Value)}.{propertyName}") {
                    Source = this,
                    Mode = BindingMode.OneWay,
                    Converter = new IntAbsoluteValueConverter() {
                        Format = format,
                    },
                };
                textBox.SetBinding(TextBox.TextProperty, binding);
            }
            bind(nameof(TimeSpan.Days), TextBoxDays);
            bind(nameof(TimeSpan.Hours), TextBoxHours, "D2");
            bind(nameof(TimeSpan.Minutes), TextBoxMinutes, "D2");
            bind(nameof(TimeSpan.Seconds), TextBoxSeconds, "D2");
            bind(nameof(TimeSpan.Milliseconds), TextBoxMilliseconds, "D3");

            /** Also trigger a event
             */
            var eventArgs = new RoutedEventArgs(ValueChangedEvent);
            RaiseEvent(eventArgs);
        }

        private static void OnValueChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args) {
            var control = (TimeSpanControl)obj;
            control.OnValueChanged(args);
        }

        private void ButtonZero_Click(object sender, RoutedEventArgs e) {
            Value = TimeSpan.Zero;
        }

        private void ButtonMin_Click(object sender, RoutedEventArgs e) {
            Value = TimeSpan.MinValue;
        }

        private void ButtonMax_Click(object sender, RoutedEventArgs e) {
            Value = TimeSpan.MaxValue;
        }

        private void TextBoxDays_LostFocus(object sender, RoutedEventArgs e) {
            if (uint.TryParse(TextBoxDays.Text.Trim(), out var newValueAbs)) {
                var newValue = Sign(Value) * (int)newValueAbs;
                Value = new TimeSpan(newValue, Value.Hours, Value.Minutes, Value.Seconds, Value.Milliseconds);
            }
            TextBoxDays.Text = Math.Abs(Value.Days).ToString();
        }

        private void TextBoxHours_LostFocus(object sender, RoutedEventArgs e) {
            if (uint.TryParse(TextBoxHours.Text.Trim(), out var newValueAbs)) {
                var newValue = Sign(Value) * (int)newValueAbs;
                Value = new TimeSpan(Value.Days, newValue, Value.Minutes, Value.Seconds, Value.Milliseconds);
            }
            TextBoxHours.Text = Math.Abs(Value.Hours).ToString("D2");
        }

        private void TextBoxMinutes_LostFocus(object sender, RoutedEventArgs e) {
            if (int.TryParse(TextBoxMinutes.Text.Trim(), out var newValueAbs)) {
                var newValue = Sign(Value) * (int)newValueAbs;
                Value = new TimeSpan(Value.Days, Value.Hours, newValue, Value.Seconds, Value.Milliseconds);
            }
            TextBoxMinutes.Text = Math.Abs(Value.Minutes).ToString("D2");
        }

        private void TextBoxSeconds_LostFocus(object sender, RoutedEventArgs e) {
            if (uint.TryParse(TextBoxSeconds.Text.Trim(), out var newValueAbs)) {
                var newValue = Sign(Value) * (int)newValueAbs;
                Value = new TimeSpan(Value.Days, Value.Hours, Value.Minutes, newValue, Value.Milliseconds);
            }
            TextBoxSeconds.Text = Math.Abs(Value.Seconds).ToString("D2");
        }

        private void TextBoxMilliseconds_LostFocus(object sender, RoutedEventArgs e) {
            if (uint.TryParse(TextBoxMilliseconds.Text.Trim(), out var newValueAbs)) {
                var newValue = Sign(Value) * (int)newValueAbs;
                Value = new TimeSpan(Value.Days, Value.Hours, Value.Minutes, Value.Seconds, newValue);
            }
            TextBoxMilliseconds.Text = Math.Abs(Value.Milliseconds).ToString("D3");
        }

        private void RadioButtonSign_CheckedOrUnchecked(object sender, RoutedEventArgs e) {
            if (RadioButtonPositive is null || RadioButtonNegative is null) {
                return;
            }

            var sign = (RadioButtonPositive.IsChecked, RadioButtonNegative.IsChecked) switch {
                (true, false) => 1,
                (false, true) => -1,
                _ => throw new InvalidOperationException(),
            };
            Value = new TimeSpan(
                days: sign * Math.Abs(Value.Days),
                hours: sign * Math.Abs(Value.Hours),
                minutes: sign * Math.Abs(Value.Minutes),
                seconds: sign * Math.Abs(Value.Seconds),
                milliseconds: sign * Math.Abs(Value.Milliseconds)
            );
        }

        private static int Sign(TimeSpan time) {
            var result = time >= TimeSpan.Zero ? 1 : -1;
            return result;
        }
    }
}
