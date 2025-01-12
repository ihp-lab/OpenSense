﻿using System;
using Microsoft.Psi;

namespace OpenSense.Components.OpenSmile {
    [Serializable]
    public class OpenSmileConfiguration : ConventionalComponentConfiguration {

        public OpenSmileInterop.Configuration raw = new OpenSmileInterop.Configuration();

        public OpenSmileInterop.Configuration Raw {
            get => raw;
            set => SetProperty(ref raw, value);
        }

        private bool mute = false;

        public bool Mute {
            get => mute;
            set => SetProperty(ref mute, value);
        }

        public override IComponentMetadata GetMetadata() => new OpenSmileMetadata();

        protected override object Instantiate(Pipeline pipeline, IServiceProvider serviceProvider) => new OpenSmile(pipeline, Raw) { 
            Mute = Mute,
        };
    }
}
