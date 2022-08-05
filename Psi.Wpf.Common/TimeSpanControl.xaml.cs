using System;
using System.Windows;
using System.Windows.Controls;

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
            if (int.TryParse(TextBoxDays.Text.Trim(), out var newValue)) {
                Value = new TimeSpan(newValue, Value.Hours, Value.Minutes, Value.Seconds, Value.Milliseconds);
            }
            TextBoxDays.Text = Value.Days.ToString();
        }

        private void TextBoxHours_LostFocus(object sender, RoutedEventArgs e) {
            if (int.TryParse(TextBoxHours.Text.Trim(), out var newValue)) {
                Value = new TimeSpan(Value.Days, newValue, Value.Minutes, Value.Seconds, Value.Milliseconds);
            }
            TextBoxHours.Text = Value.Hours.ToString("D2");
        }

        private void TextBoxMinutes_LostFocus(object sender, RoutedEventArgs e) {
            if (int.TryParse(TextBoxMinutes.Text.Trim(), out var newValue)) {
                Value = new TimeSpan(Value.Days, Value.Hours, newValue, Value.Seconds, Value.Milliseconds);
            }
            TextBoxMinutes.Text = Value.Minutes.ToString("D2");
        }

        private void TextBoxSeconds_LostFocus(object sender, RoutedEventArgs e) {
            if (int.TryParse(TextBoxSeconds.Text.Trim(), out var newValue)) {
                Value = new TimeSpan(Value.Days, Value.Hours, Value.Minutes, newValue, Value.Milliseconds);
            }
            TextBoxSeconds.Text = Value.Seconds.ToString("D2");
        }

        private void TextBoxMilliseconds_LostFocus(object sender, RoutedEventArgs e) {
            if (int.TryParse(TextBoxMilliseconds.Text.Trim(), out var newValue)) {
                Value = new TimeSpan(Value.Days, Value.Hours, Value.Minutes, Value.Seconds, newValue);
            }
            TextBoxMilliseconds.Text = Value.Milliseconds.ToString("D3");
        }
    }
}
