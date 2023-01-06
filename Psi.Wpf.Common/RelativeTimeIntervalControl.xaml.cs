using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using Microsoft.Psi;

namespace OpenSense.WPF.Component.Psi.Common {
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
            IntervalEndpointControlLeft.MinOrMax = TimeSpan.MinValue;
            IntervalEndpointControlRight.MinOrMax = TimeSpan.MaxValue;
        }

        private void OnValueChanged(DependencyPropertyChangedEventArgs args) {
            /** Redo bindings, otherwise the Value in IntervalEndpointControl_TimeSpan will not be updated.
             *  Create a new Binding object and bind it, because GetBindingExpression() will return null here.
             */
            void bind(string endpointName, IntervalEndpointControl_TimeSpan intervalControl) {
                var binding = new Binding($"{nameof(Value)}.{endpointName}") {
                    Source = this,
                    Mode = BindingMode.OneWay,
                };
                intervalControl.SetBinding(IntervalEndpointControl_TimeSpan.ValueProperty, binding);
            }
            bind(nameof(RelativeTimeInterval.LeftEndpoint), IntervalEndpointControlLeft);
            bind(nameof(RelativeTimeInterval.RightEndpoint), IntervalEndpointControlRight);

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

        private void IntervalEndpointControlLeft_ValueChanged(object sender, RoutedEventArgs e) {
            Value = new RelativeTimeInterval(IntervalEndpointControlLeft.Value, Value.RightEndpoint);
        }

        private void IntervalEndpointControlRight_ValueChanged(object sender, RoutedEventArgs e) {
            Value = new RelativeTimeInterval(Value.LeftEndpoint, IntervalEndpointControlRight.Value);
        }
    }
}
