using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using Microsoft.Psi;
using OpenSense.WPF.Components.Controls;

namespace OpenSense.WPF.Components.Psi {
    public partial class IntervalEndpointControl_TimeSpan : UserControl {

        #region Dependency Properties
        public static readonly DependencyProperty ValueProperty = DependencyProperty.Register(
            nameof(Value),
            typeof(IntervalEndpoint<TimeSpan>),
            typeof(IntervalEndpointControl_TimeSpan),
            new PropertyMetadata(defaultValue: new IntervalEndpoint<TimeSpan>(TimeSpan.MaxValue), OnValueChanged)
        );

        public static readonly DependencyProperty MinOrMaxProperty = DependencyProperty.Register(
            nameof(MinOrMax),
            typeof(TimeSpan),
            typeof(IntervalEndpointControl_TimeSpan),
            new PropertyMetadata(defaultValue: TimeSpan.MaxValue, OnMinOrMaxChanged)
        );
        #endregion

        #region Routed Events
        public static readonly RoutedEvent ValueChangedEvent = EventManager.RegisterRoutedEvent(
            name: nameof(ValueChanged),
            routingStrategy: RoutingStrategy.Direct,
            handlerType: typeof(RoutedEventHandler),
            ownerType: typeof(IntervalEndpointControl_TimeSpan)
        );
        #endregion

        #region CLR Dependency Properties
        public IntervalEndpoint<TimeSpan> Value {
            get => (IntervalEndpoint<TimeSpan>)GetValue(ValueProperty);
            set {
                var isEqual = Value.Bounded == value.Bounded 
                    && Value.Point == value.Point 
                    && Value.Inclusive == value.Inclusive;
                if (isEqual) {
                    return;
                }
                SetValue(ValueProperty, value);
            }
        }

        public TimeSpan MinOrMax {
            get => (TimeSpan)GetValue(MinOrMaxProperty);
            set {
                if (MinOrMax == value) {
                    return;
                }
                SetValue(MinOrMaxProperty, value);
            }
        }
        #endregion

        #region CLR Routed Events
        public event RoutedEventHandler ValueChanged {
            add => AddHandler(ValueChangedEvent, value);
            remove => RemoveHandler(ValueChangedEvent, value);
        }
        #endregion

        #region Constructors
        public IntervalEndpointControl_TimeSpan() {
            InitializeComponent();
        }
        #endregion

        #region Static Event Implementations
        private static void OnValueChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args) {
            var control = (IntervalEndpointControl_TimeSpan)obj;
            control.OnValueChanged(args);
        }

        private static void OnMinOrMaxChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args) {
            var control = (IntervalEndpointControl_TimeSpan)obj;
            control.OnMinOrMaxChanged(args);
        }
        #endregion

        #region Instance Event Implementations
        private void OnValueChanged(DependencyPropertyChangedEventArgs args) {
            /** Redo bindings, otherwise the Value in TimeSpanControl will not be updated.
             *  Create a new Binding object and bind it, because GetBindingExpression() will return null here.
             */
            Binding getBinding(string propertyName) {
                var binding = new Binding($"{nameof(Value)}.{propertyName}") {
                    Source = this,
                    Mode = BindingMode.OneWay,
                };
                return binding;
            }
            CheckBoxBounded.SetBinding(ToggleButton.IsCheckedProperty, getBinding(nameof(IntervalEndpoint<TimeSpan>.Bounded)));
            TimeSpanControlPoint.SetBinding(TimeSpanControl.ValueProperty, getBinding(nameof(IntervalEndpoint<TimeSpan>.Point)));
            CheckBoxInclusive.SetBinding(ToggleButton.IsCheckedProperty, getBinding(nameof(IntervalEndpoint<TimeSpan>.Inclusive)));

            /** Also trigger a event
             */
            var eventArgs = new RoutedEventArgs(ValueChangedEvent);
            RaiseEvent(eventArgs);
        }

        private void OnMinOrMaxChanged(DependencyPropertyChangedEventArgs args) {
            //Nothing
        }
        #endregion

        private void CheckBoxBounded_IsCheckedChanged(object sender, RoutedEventArgs e) {
            if (CheckBoxBounded.IsChecked == true) {
                Value = new IntervalEndpoint<TimeSpan>(Value.Point, Value.Inclusive);
            } else {
                Value = new IntervalEndpoint<TimeSpan>(MinOrMax);
            }
        }

        private void TimeSpanControlPoint_ValueChanged(object sender, RoutedEventArgs e) {
            if (Value.Bounded) {
                Value = new IntervalEndpoint<TimeSpan>(TimeSpanControlPoint.Value, Value.Inclusive);
            } else {
                Value = new IntervalEndpoint<TimeSpan>(MinOrMax);
            }
        }

        private void CheckBoxInclusive_IsCheckedChanged(object sender, RoutedEventArgs e) {
            if (Value.Bounded) {
                Value = new IntervalEndpoint<TimeSpan>(Value.Point, CheckBoxInclusive.IsChecked == true);
            } else {
                Value = new IntervalEndpoint<TimeSpan>(MinOrMax);
            }
        }
    }
}
