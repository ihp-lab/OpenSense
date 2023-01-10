﻿using System.Composition;
using System.Windows;
using OpenSense.Components.Contract;
using OpenSense.Components.Psi.AzureKinect;
using OpenSense.WPF.Components.Contract;

namespace OpenSense.WPF.Components.Psi.AzureKinect {
    [Export(typeof(IConfigurationControlCreator))]
    public class AzureKinectSensorConfigurationControlCreator : IConfigurationControlCreator {

        public bool CanCreate(ComponentConfiguration configuration) => configuration is AzureKinectSensorConfiguration;

        public UIElement Create(ComponentConfiguration configuration) => new AzureKinectSensorConfigurationControl() { DataContext = configuration };
    }
}