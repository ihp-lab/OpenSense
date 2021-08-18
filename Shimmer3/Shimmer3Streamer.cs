using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using Microsoft.Extensions.Logging;
using Microsoft.Psi;
using ShimmerAPI;

namespace OpenSense.Component.Shimmer3 {
    public class Shimmer3Streamer: IDisposable, INotifyPropertyChanged {

        protected const int Shimmer3ClockFrequency = 32768;//CalibrateTimeStamp() in ShimmerDevice.cs

        protected static readonly DateTime UnixEpoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        private readonly DeviceConfiguration _config;

        #region Settings
        private ILogger logger;

        public ILogger Logger {
            protected get => logger;
            set => SetProperty(ref logger, value);
        }

        private TimeSpan bufferTimeSpan = TimeSpan.FromMilliseconds(100);

        /// <summary>
        /// Time allowed for re-ordering out-of-order packets.
        /// Timeout packets will be dropped.
        /// Will add delay.
        /// </summary>
        public TimeSpan BufferTimeSpan {
            get => bufferTimeSpan;
            set => SetProperty(ref bufferTimeSpan, value);
        }

        private bool doNotPostIfNoSubscriber = false;

        public bool DoNotPostIfNoSubscriber {
            get => doNotPostIfNoSubscriber;
            set => SetProperty(ref doNotPostIfNoSubscriber, value);
        }
        #endregion

        #region Output Channels

        /// <remarks>
        /// Only given once when pipeline is started.
        /// </remarks>
        public Emitter<double> SampleRateOut { get; }

        /// <remarks>
        /// Only given once when pipeline is started.
        /// </remarks>
        public Emitter<int> BaudRateOut { get; }


        #region Internal ADC
        /// <summary>
        /// Internal ADC
        /// Unit: mVolt
        /// Can be set as PPG output.
        /// </summary>
        public Emitter<double> InternalAdc1 { get; }

        /// <summary>
        /// Internal ADC
        /// Unit: mVolt
        /// Can be set as PPG output.
        /// </summary>
        public Emitter<double> InternalAdc12 { get; }

        /// <summary>
        /// Internal ADC
        /// Unit: mVolt
        /// Can be set as PPG output.
        /// </summary>
        public Emitter<double> InternalAdc13 { get; }

        /// <summary>
        /// Internal ADC
        /// Unit: mVolt
        /// Can be set as PPG output.
        /// </summary>
        public Emitter<double> InternalAdc14 { get; }
        #endregion

        #region ECG
        /// <summary>
        /// Electrocardiography left leg / right arm
        /// Unit: mVolt
        /// </summary>
        public Emitter<double> ECG_LL_RA { get; }

        /// <summary>
        /// Electrocardiography left arm / right arm
        /// Unit: mVolt
        /// </summary>
        public Emitter<double> ECG_LA_RA { get; }

        /// <summary>
        /// Electrocardiography Vx (V1-V6) / right leg
        /// Unit: mVolt
        /// </summary>
        public Emitter<double> ECG_Vx_RL { get; }

        /// <summary>
        /// Unit: mVolt
        /// </summary>
        public Emitter<double> ExG2_CH1 { get; }
        #endregion

        #region GSR
        /// <summary>
        /// Unit: Kilo Ohms
        /// </summary>
        public Emitter<double> GSR_Resistance { get; }

        /// <summary>
        /// Unit: Micro Siemens
        /// Converted from resistance internaly
        /// </summary>
        public Emitter<double> GSR_Conductance { get; }
        #endregion
        #endregion

        protected ShimmerLogAndStreamSystemSerialPort device;

        protected DateTimeOffset firstDataPacketTime = DateTimeOffset.MinValue;

        public Shimmer3Streamer(Pipeline pipeline, DeviceConfiguration configuration) {
            _config = configuration ?? throw new ArgumentNullException(nameof(configuration));

            SampleRateOut = pipeline.CreateEmitter<double>(this, nameof(SampleRateOut));
            BaudRateOut = pipeline.CreateEmitter<int>(this, nameof(BaudRateOut));

            InternalAdc1 = pipeline.CreateEmitter<double>(this, nameof(InternalAdc1));
            InternalAdc12 = pipeline.CreateEmitter<double>(this, nameof(InternalAdc12));
            InternalAdc13 = pipeline.CreateEmitter<double>(this, nameof(InternalAdc13));
            InternalAdc14 = pipeline.CreateEmitter<double>(this, nameof(InternalAdc14));

            ECG_LL_RA = pipeline.CreateEmitter<double>(this, nameof(ECG_LL_RA));
            ECG_LA_RA = pipeline.CreateEmitter<double>(this, nameof(ECG_LA_RA));
            ECG_Vx_RL = pipeline.CreateEmitter<double>(this, nameof(ECG_Vx_RL));
            ExG2_CH1 = pipeline.CreateEmitter<double>(this, nameof(ExG2_CH1));

            GSR_Resistance = pipeline.CreateEmitter<double>(this, nameof(GSR_Resistance));
            GSR_Conductance = pipeline.CreateEmitter<double>(this, nameof(GSR_Conductance));

            pipeline.PipelineRun += OnPipelineRun;
        }

        private void OnPipelineRun(object sender, PipelineRunEventArgs args) {
            Connect();
        }

        private void CloseOutputEmitters() {
            var time = DateTime.UtcNow;

            InternalAdc1.Close(time);
            InternalAdc12.Close(time);
            InternalAdc13.Close(time);
            InternalAdc14.Close(time);

            ECG_LL_RA.Close(time);
            ECG_LA_RA.Close(time);
            ECG_Vx_RL.Close(time);
            ExG2_CH1.Close(time);
        }

        private void Connect() {
            var time = DateTime.UtcNow;

            device = new ShimmerLogAndStreamSystemSerialPort("DUMMY_DEVICE_NAME", _config.SerialPortName);
            device.UICallback += OnShimmerDeviceEvent;
            device.mEnableTimeStampAlignmentCheck = false;//process out-of-order packets ourselves
            device.WriteInternalExpPower(_config.InternalExpPower ? 1 : 0);
            device.SetExpPower(_config.InternalExpPower);
            device.WriteBaudRate(_config.BaudRateIndex);

            SampleRateOut.Post(_config.SampleRate, time);
            SampleRateOut.Close(time);
            BaudRateOut.Post(_config.BaudRate, time);
            BaudRateOut.Close(time);

            device.StartConnectThread();
        }

        private void Disconnect() {
            if (device is null) {
                return;
            }
            if (device.GetState() == ShimmerBluetooth.SHIMMER_STATE_STREAMING && device.GetFirmwareIdentifier() != 3) {
                device.StopStreaming();
            }
            device.Disconnect();
            device = null;
        }

        private void OnShimmerDeviceEvent(object sender, EventArgs args) {
            var s = (ShimmerBluetooth)sender;
            var e = (CustomEventArgs)args;
            var state = (ShimmerBluetooth.ShimmerIdentifier)e.getIndicator();
            switch (state) {
                case ShimmerBluetooth.ShimmerIdentifier.MSG_IDENTIFIER_STATE_CHANGE:
                    ProcessStateChangedEvent(s, e);
                    break;
                case ShimmerBluetooth.ShimmerIdentifier.MSG_IDENTIFIER_NOTIFICATION_MESSAGE:
                    ProcessNotificationMessageEvent(s, e);
                    break;
                case ShimmerBluetooth.ShimmerIdentifier.MSG_IDENTIFIER_DATA_PACKET:
                    ProcessDataPacketEvent(s, e);
                    break;
                case ShimmerBluetooth.ShimmerIdentifier.MSG_IDENTIFIER_PACKET_RECEPTION_RATE:
                    ProcessIdentifierPacketReceptionRateEvent(s, e);
                    break;
                default:
                    Logger?.LogWarning("Received an event with unsupported Shimmer identifier {id}", (int)state);
                    break;
            }
        }

        private void ProcessStateChangedEvent(ShimmerBluetooth device, CustomEventArgs args) {
            var state = (int)args.getObject();
            switch (state) {
                case ShimmerBluetooth.SHIMMER_STATE_CONNECTED:
                    Logger?.LogInformation("Shimmer connected via Bluetooth {portName}", device.GetShimmerAddress());
                    //send start streaming message only after device is connected
                    device.StartStreaming();//.StartStreamingAndLog() is also available
                    break;
                case ShimmerBluetooth.SHIMMER_STATE_CONNECTING:
                    Logger?.LogInformation("Connecting Shimmer via Bluetooth {portName}", device.GetShimmerAddress());
                    break;
                case ShimmerBluetooth.SHIMMER_STATE_NONE:
                    Logger?.LogWarning("Shimmer disconnected");
                    CloseOutputEmitters();
                    break;
                case ShimmerBluetooth.SHIMMER_STATE_STREAMING:
                    Logger?.LogInformation("Shimmer started streaming via Bluetooth {portName}", device.GetShimmerAddress());
                    break;
            }
        }

        private void ProcessNotificationMessageEvent(ShimmerBluetooth device, CustomEventArgs args) {
            var message = (string)args.getObject();
            var state = (ShimmerLogAndStream.ShimmerSDBTMinorIdentifier)args.getMinorIndication();
            switch (state) {
                case ShimmerLogAndStream.ShimmerSDBTMinorIdentifier.MSG_WARNING:
                    Logger?.LogWarning("Received a Shimmer warning \"{message}\" via Bluetooth {portName}", message, device.GetShimmerAddress());
                    break;
                case ShimmerLogAndStream.ShimmerSDBTMinorIdentifier.MSG_EXTRA_REMOVABLE_DEVICES_DETECTED:
                    Logger?.LogInformation("Received MSG_EXTRA_REMOVABLE_DEVICES_DETECTED via Bluetooth {portName}", device.GetShimmerAddress());
                    break;
                case ShimmerLogAndStream.ShimmerSDBTMinorIdentifier.MSG_ERROR:
                    Logger?.LogError("Received a Shimmer error \"{message}\" via Bluetooth {portName}", message, device.GetShimmerAddress());
                    break;
                default:
                    switch (message) {
                        case "Connection lost":
                        case "Unable to connect to specified port":
                            Logger?.LogInformation("Received a Shimmer message \"{message}\" via Bluetooth {portName}, will shutdown Shimmer streamer", message, device.GetShimmerAddress());
                            CloseOutputEmitters();
                            break;
                        default:
                            Logger?.LogInformation("Received a unrecognized Shimmer message \"{message}\" via Bluetooth {portName}", message, device.GetShimmerAddress());
                            break;
                    }
                    break;
            }
        }

        private void ProcessDataPacketEvent(ShimmerBluetooth device, CustomEventArgs args) {
            var aggr = (ObjectCluster)args.getObject();

            /* Timestamps it give are totally wrong! Unusable! These timestamps are adjusted by CalibrateTimeStamp() in ShimmerDevice.cs.
            var elapsed = aggr.GetData(ShimmerConfiguration.SignalNames.TIMESTAMP, ShimmerConfiguration.SignalFormats.CAL).Data;//in mSec
            var timestamp = aggr.GetData(ShimmerConfiguration.SignalNames.SYSTEM_TIMESTAMP, ShimmerConfiguration.SignalFormats.CAL).Data;
            var originatingTime = UnixEpoch + TimeSpan.FromMilliseconds(timestamp);
            */
            /* also not working, the raw timestamp does not make sense
            var clock = aggr.RawTimeStamp;
            if (firstDataPacketTime == DateTimeOffset.MinValue) {
                firstDataPacketTime = DateTimeOffset.UtcNow;
            }
            var elapsedSeconds = (double)clock / Shimmer3ClockFrequency;
            var originatingTime = firstDataPacketTime + TimeSpan.FromSeconds(elapsedSeconds);
            Debug.WriteLine($"{(DateTimeOffset.UtcNow - firstDataPacketTime).TotalSeconds:F6}:{clock}");
            */
            var originatingTime = DateTimeOffset.UtcNow;
            
            PostData(aggr, Shimmer3Configuration.SignalNames.INTERNAL_ADC_A1, InternalAdc1, originatingTime);
            PostData(aggr, Shimmer3Configuration.SignalNames.INTERNAL_ADC_A12, InternalAdc12, originatingTime);
            PostData(aggr, Shimmer3Configuration.SignalNames.INTERNAL_ADC_A13, InternalAdc13, originatingTime);
            PostData(aggr, Shimmer3Configuration.SignalNames.INTERNAL_ADC_A14, InternalAdc14, originatingTime);

            PostData(aggr, Shimmer3Configuration.SignalNames.ECG_LL_RA, ECG_LL_RA, originatingTime);
            PostData(aggr, Shimmer3Configuration.SignalNames.ECG_LA_RA, ECG_LA_RA, originatingTime);
            PostData(aggr, Shimmer3Configuration.SignalNames.ECG_VX_RL, ECG_Vx_RL, originatingTime);
            PostData(aggr, Shimmer3Configuration.SignalNames.EXG2_CH1, ExG2_CH1, originatingTime);
            
            PostData(aggr, Shimmer3Configuration.SignalNames.GSR, GSR_Resistance, originatingTime);
            PostData(aggr, Shimmer3Configuration.SignalNames.GSR_CONDUCTANCE, GSR_Conductance, originatingTime);
        }

        private void ProcessIdentifierPacketReceptionRateEvent(ShimmerBluetooth device, CustomEventArgs args) {
            var rate = (double)args.getObject();
            //do nothing
        }

        protected void PostData(ObjectCluster aggr, string signalName, Emitter<double> emitter, DateTimeOffset originatingTime) {
            if (DoNotPostIfNoSubscriber && !emitter.HasSubscribers) {
                return;
            }
            var wrapper = aggr.GetData(signalName, ShimmerConfiguration.SignalFormats.CAL);
            if (wrapper is null) {
                return;
            }
            var data = wrapper.Data;
            //Debug.WriteLine($"{originatingTime:O} - {signalName}: {data}");
            var timeToPost = originatingTime;
            if (originatingTime <= emitter.LastEnvelope.OriginatingTime) {
                timeToPost = emitter.LastEnvelope.OriginatingTime + TimeSpan.FromMilliseconds(1);//since the raw timestamp itself doen't make sense, we do not further reorder packets
            }
            emitter.Post(data, timeToPost.DateTime);
        }

        #region INotifyPropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;

        private void SetProperty<T>(ref T field, T value, [CallerMemberName] string propertyName = null) {
            if (!EqualityComparer<T>.Default.Equals(field, value)) {
                field = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            }
        }
        #endregion

        #region IDisposable
        public void Dispose() {
            Disconnect();
        }
        #endregion
    }
}
