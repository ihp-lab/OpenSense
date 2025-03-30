﻿using System.Composition;
using System.Windows;
using OpenSense.Components.Psi.AzureKinect.Visualizer;
using OpenSense.WPF.Components;

namespace OpenSense.WPF.Components.Psi.AzureKinect.Visualizer {
    [Export(typeof(IInstanceControlCreator))]
    public class AzureKinectBodyTrackerVisualizerInstanceControlCreator : IInstanceControlCreator {

        public bool CanCreate(object instance) => instance is AzureKinectBodyTrackerVisualizer;

        public UIElement Create(object instance) => new AzureKinectBodyTrackerVisualizerInstanceControl() { DataContext = instance };
    }
}
