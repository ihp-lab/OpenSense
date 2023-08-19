using System;
using Microsoft.Psi;
using OpenSense.Components.Contract;

namespace OpenSense.Components.Builtin {
    [Serializable]
    public sealed class DefaultValueInjectorConfiguration : GenericComponentConfiguration_OneParam {

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

        public override IComponentMetadata GetMetadata() => new DefaultValueInjectorMetadata();

        protected override object Instantiate<T>(Pipeline pipeline, IServiceProvider serviceProvider) {
            var result = new DefaultValueInjector<T>(pipeline) { 
                DefaultValue = default,
                InputAbsenceTolerance = InputAbsenceTolerance,
                ReferenceAbsenceTolerance = ReferenceAbsenceTolerance,
            };
            return result;
        }
    }
}
