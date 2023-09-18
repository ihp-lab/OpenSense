using System;
using System.Composition;

namespace OpenSense.Components.Shimmer3 {
    [Export(typeof(IComponentMetadata))]
    public class Shimmer3StreamerMetadata : ConventionalComponentMetadata {

        public override string Description => "Shimmer 3 sensor. The device should be paird via Bluetooth.";

        protected override Type ComponentType => typeof(Shimmer3Streamer);

        public override string Name => "Shimmer 3 Sensor";

        protected override string GetPortDescription(string portName) {
            switch (portName) {
                case nameof(Shimmer3Streamer.SampleRateOut):
                    return "Sample rate. Send only once.";
                case nameof(Shimmer3Streamer.BaudRateOut):
                    return "Baud rate. Send only once.";
                case nameof(Shimmer3Streamer.InternalAdc1):
                    return "Internal ADC 1 signal. The unit is mVolt. Only available when the ADC is enabled.";
                case nameof(Shimmer3Streamer.InternalAdc12):
                    return "Internal ADC 12 signal. The unit is mVolt. Only available when the ADC is enabled.";
                case nameof(Shimmer3Streamer.InternalAdc13):
                    return "Internal ADC 13 signal. The unit is mVolt. Only available when the ADC is enabled.";
                case nameof(Shimmer3Streamer.InternalAdc14):
                    return "Internal ADC 14 signal. The unit is mVolt. Only available when the ADC is enabled.";
                case nameof(Shimmer3Streamer.ECG_LL_RA):
                    return "The unit is mVolt. Only available when the function is supported and enabled.";
                case nameof(Shimmer3Streamer.ECG_LA_RA):
                    return "The unit is mVolt. Only available when the function is supported and enabled.";
                case nameof(Shimmer3Streamer.ECG_Vx_RL):
                    return "The unit is mVolt. Only available when the function is supported and enabled.";
                case nameof(Shimmer3Streamer.ExG2_CH1):
                    return "The unit is mVolt. Only available when the function is supported and enabled.";
                case nameof(Shimmer3Streamer.GSR_Resistance):
                    return "The unit is kOhms. Only available when the function is supported and enabled.";
                case nameof(Shimmer3Streamer.GSR_Conductance):
                    return "The unit is Micro Siemens. Only available when the function is supported and enabled.";
                default:
                    return null;
            }
        }

        public override ComponentConfiguration CreateConfiguration() => new Shimmer3StreamerConfiguration();
    }
}
