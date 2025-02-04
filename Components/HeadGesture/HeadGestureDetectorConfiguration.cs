﻿using System;
using Microsoft.Psi;

namespace OpenSense.Components.HeadGesture {
    [Serializable]
    public class HeadGestureDetectorConfiguration : ConventionalComponentConfiguration {

        public override IComponentMetadata GetMetadata() => new HeadGestureDetectorMetadata();

        protected override object Instantiate(Pipeline pipeline, IServiceProvider serviceProvider) => new HeadGestureDetector(pipeline);
    }
}
