﻿#nullable enable

using System;
using Microsoft.Extensions.Logging;
using Microsoft.Psi;

namespace OpenSense.Components.Builtin {
    [Serializable]
    public sealed class DefaultValueInjectorConfiguration : GenericComponentConfiguration_OneParam {

        private static readonly DefaultValueInjectorMetadata Metadata = new DefaultValueInjectorMetadata();


        /*
        private T? defaultValue = default;

        public T? DefaultValue {
            get => defaultValue;
            set => SetProperty(ref defaultValue, value);
        }
        */

        private TimeSpan inputAbsenceTolerance = TimeSpan.FromMilliseconds(2 * 1000 / 30);

        public TimeSpan InputAbsenceTolerance {
            get => inputAbsenceTolerance;
            set => SetProperty(ref inputAbsenceTolerance, value);
        }

        private TimeSpan referenceAbsenceTolerance = TimeSpan.FromMilliseconds(2 * 1000 / 30);

        public TimeSpan ReferenceAbsenceTolerance {
            get => referenceAbsenceTolerance;
            set => SetProperty(ref referenceAbsenceTolerance, value);
        }

        private TimeSpan stoppingTimeout = TimeSpan.FromSeconds(1);

        public TimeSpan StoppingTimeout {
            get => stoppingTimeout;
            set => SetProperty(ref stoppingTimeout, value);
        }

        public override IComponentMetadata GetMetadata() => Metadata;

        protected override object Instantiate<T>(Pipeline pipeline, IServiceProvider? serviceProvider) {
            var result = new DefaultValueInjector<T>(pipeline) {
                Logger = (serviceProvider?.GetService(typeof(ILoggerFactory)) as ILoggerFactory)?.CreateLogger(Name),
                DefaultValue = default,
                InputAbsenceTolerance = InputAbsenceTolerance,
                ReferenceAbsenceTolerance = ReferenceAbsenceTolerance,
                StoppingTimeout = StoppingTimeout,
            };
            return result;
        }
    }
}
