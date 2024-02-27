using System;
using System.Numerics;
using System.Windows;
using System.Windows.Media;
using System.Windows.Threading;
using Microsoft.Psi;
using Microsoft.Psi.Components;

namespace OpenSense.WPF.Widgets.DisplayPoiEstimatorBuilder {
    internal sealed class DisplayCoordianteGenerator : Generator, IProducer<Vector2> {

        private readonly EllipseGeometry _ellipse;

        private readonly FrameworkElement _frameworkElement;

        private readonly Dispatcher _dispatcher;

        private readonly TimeSpan _resolution;

        public Emitter<Vector2> Out { get; }

        public DisplayCoordianteGenerator(Pipeline pipeline, EllipseGeometry ellipse, FrameworkElement frameworkElement, Dispatcher dispatcher, double fps) : base(pipeline, isInfiniteSource: true) {
            _ellipse = ellipse;
            _frameworkElement = frameworkElement;
            _dispatcher = dispatcher;
            _resolution = TimeSpan.FromSeconds(1) / fps;

            Out = pipeline.CreateEmitter<Vector2>(this, nameof(Out));
        }

        #region Generator
        protected override DateTime GenerateNext(DateTime currentTime) {
            var now = DateTime.UtcNow;
            _dispatcher.Invoke(() => {
                var raw = _ellipse.Center;
                var relativeX = (float)(raw.X / _frameworkElement.ActualWidth);
                var relativeY = (float)(raw.Y / _frameworkElement.ActualHeight);
                var relative = new Vector2(relativeX, relativeY);
                Out.Post(relative, now);
            });
            return now + _resolution;
        }
        #endregion
    }
}
