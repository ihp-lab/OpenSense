using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using Microsoft.Psi;

namespace OpenSense.Wpf.Component.Psi.Common {
    public partial class RelativeTimeIntervalControl : UserControl {

        public static readonly DependencyProperty ValueProperty = DependencyProperty.Register(
            nameof(Value), 
            typeof(RelativeTimeInterval), 
            typeof(RelativeTimeIntervalControl), 
            new PropertyMetadata(defaultValue: RelativeTimeInterval.Empty, OnValueChanged)
        );

        public static readonly RoutedEvent ValueChangedEvent = EventManager.RegisterRoutedEvent(
            name: nameof(ValueChanged),
            routingStrategy: RoutingStrategy.Direct,
            handlerType: typeof(RoutedEventHandler),
            ownerType: typeof(RelativeTimeIntervalControl)
        );

        public RelativeTimeInterval Value {
            get => (RelativeTimeInterval)GetValue(ValueProperty);
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

        public RelativeTimeIntervalControl() {
            InitializeComponent();
        }

        private void OnValueChanged(DependencyPropertyChangedEventArgs args) {
            /** Redo bindings, otherwise the Value in TimeSpanControl will not be updated.
             *  Create a new Binding object and bind it, because GetBindingExpression() will return null here.
             */
            var leftBinding = new Binding($"{nameof(Value)}.{nameof(RelativeTimeInterval.LeftEndpoint)}.{nameof(IntervalEndpoint<TimeSpan>.Point)}") {
                Source = this,
                Mode = BindingMode.OneWay,
            };
            TimeSpanLeft.SetBinding(TimeSpanControl.ValueProperty, leftBinding);
            var rightBinding = new Binding($"{nameof(Value)}.{nameof(RelativeTimeInterval.RightEndpoint)}.{nameof(IntervalEndpoint<TimeSpan>.Point)}") {
                Source = this,
                Mode = BindingMode.OneWay,
            };
            TimeSpanRight.SetBinding(TimeSpanControl.ValueProperty, rightBinding);
            /** Also trigger a event
             */
            var eventArgs = new RoutedEventArgs(ValueChangedEvent);
            RaiseEvent(eventArgs);
        }

        private static void OnValueChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args) {
            var control = (RelativeTimeIntervalControl)obj;
            control.OnValueChanged(args);
        }

        private void ButtonEmpty_Click(object sender, RoutedEventArgs e) {
            Value = RelativeTimeInterval.Empty;
        }

        private void ButtonZero_Click(object sender, RoutedEventArgs e) {
            Value = RelativeTimeInterval.Zero;
        }

        private void ButtonInfinite_Click(object sender, RoutedEventArgs e) {
            Value = RelativeTimeInterval.Infinite;
        }

        private void ButtonPast_Click(object sender, RoutedEventArgs e) {
            Value = RelativeTimeInterval.Past();
        }

        private void ButtonFuture_Click(object sender, RoutedEventArgs e) {
            Value = RelativeTimeInterval.Future();
        }

        private void CheckBoxLeftBounded_Checked(object sender, RoutedEventArgs e) {
            var oldEndPoint = Value.LeftEndpoint;
            var newEndpoint = new IntervalEndpoint<TimeSpan>(oldEndPoint.Point, oldEndPoint.Inclusive);
            Value = new RelativeTimeInterval(newEndpoint, Value.RightEndpoint);
        }

        private void CheckBoxLeftBounded_Unchecked(object sender, RoutedEventArgs e) {
            var newEndpoint = new IntervalEndpoint<TimeSpan>(TimeSpan.MinValue);
            Value = new RelativeTimeInterval(newEndpoint, Value.RightEndpoint);
        }

        private void CheckBoxRightBounded_Checked(object sender, RoutedEventArgs e) {
            var oldEndPoint = Value.RightEndpoint;
            var newEndpoint = new IntervalEndpoint<TimeSpan>(oldEndPoint.Point, oldEndPoint.Inclusive);
            Value = new RelativeTimeInterval(Value.LeftEndpoint, newEndpoint);
        }

        private void CheckBoxRightBounded_Unchecked(object sender, RoutedEventArgs e) {
            var newEndpoint = new IntervalEndpoint<TimeSpan>(TimeSpan.MaxValue);
            Value = new RelativeTimeInterval(Value.LeftEndpoint, newEndpoint);
        }

        private void TimeSpanLeft_Changed(object sender, RoutedEventArgs e) {
            var oldEndPoint = Value.LeftEndpoint;
            var newEndpoint = oldEndPoint.Bounded ?
                new IntervalEndpoint<TimeSpan>(TimeSpanLeft.Value, oldEndPoint.Inclusive)
                :
                new IntervalEndpoint<TimeSpan>(TimeSpan.MinValue);
            Value = new RelativeTimeInterval(newEndpoint, Value.RightEndpoint);
        }

        private void TimeSpanRight_Changed(object sender, RoutedEventArgs e) {
            var oldEndPoint = Value.RightEndpoint;
            var newEndpoint = oldEndPoint.Bounded ?
                new IntervalEndpoint<TimeSpan>(TimeSpanRight.Value, oldEndPoint.Inclusive)
                :
                new IntervalEndpoint<TimeSpan>(TimeSpan.MaxValue);
            Value = new RelativeTimeInterval(Value.LeftEndpoint, newEndpoint);
        }

        private void CheckBoxLeftInclusive_Changed(object sender, RoutedEventArgs e) {
            var oldEndPoint = Value.LeftEndpoint;
            var newEndpoint = oldEndPoint.Bounded ?
                new IntervalEndpoint<TimeSpan>(oldEndPoint.Point, CheckBoxLeftInclusive.IsChecked == true)
                :
                new IntervalEndpoint<TimeSpan>(TimeSpan.MinValue);
            Value = new RelativeTimeInterval(newEndpoint, Value.RightEndpoint);
        }

        private void CheckBoxRightInclusive_Changed(object sender, RoutedEventArgs e) {
            var oldEndPoint = Value.RightEndpoint;
            var newEndpoint = oldEndPoint.Bounded ?
                new IntervalEndpoint<TimeSpan>(oldEndPoint.Point, CheckBoxRightInclusive.IsChecked == true)
                :
                new IntervalEndpoint<TimeSpan>(TimeSpan.MaxValue);
            Value = new RelativeTimeInterval(Value.LeftEndpoint, newEndpoint);
        }
    }
}
