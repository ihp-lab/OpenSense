using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Windows.Controls;
using OpenSense.PipelineBuilder.Configurations;

namespace OpenSense.PipelineBuilder.Controls {
    public static class ControlFactory {

        public static UserControl CreateConfigurationControl(InstanceConfiguration config, IList<InstanceConfiguration> instances) {
            switch (config) {
                #region Psi
                case LocalExporterConfiguration _:
                    return new Configuration.LocalExporterControl();
                case RemoteExporterConfiguration _:
                    return new Configuration.RemoteExporterControl();
                case StreamWriterImplConfiguration _:
                    return new Configuration.StreamWriterImplControl(instances);
                case LocalImporterConfiguration _:
                    return new Configuration.LocalImporterControl();
                case RemoteImporterConfiguration _:
                    return new Configuration.RemoteImporterControl();
                case StreamReaderImplConfiguration _:
                    return new Configuration.StreamReaderImplControl(instances);
                case JoinOperatorConfiguration _:
                    return new Configuration.JoinOperatorControl();

                case AudioCaptureConfiguration _:
                    return new Configuration.AudioCaptureControl();
                case MediaCaptureConfiguration _:
                    return new Configuration.MediaCaptureControl();
                case AzureKinectSensorConfiguration _:
                    return new Configuration.AzureKinectSensorControl();
                case MediaSourceConfiguration _:
                    return new Configuration.MediaSourceControl();
                case WaveFileAudioSourceConfiguration _:
                    return new Configuration.WaveFileAudioSourceControl();
#if NETFRAMEWORK
                case SystemVoiceActivityDetectorConfiguration _:
                    return new Configuration.SystemVoiceActivityDetectorControl();
                case SystemSpeechRecognizerConfiguration _:
                    return new Configuration.SystemSpeechRecognizerControl();
                case KinectSensorConfiguration _:
                    return new Configuration.KinectSensorControl();
#endif
                case AudioResamplerConfiguration _:
                    return new Configuration.AudioResamplerControl();
                case AzureSpeechRecognizerConfiguration _:
                    return new Configuration.AzureSpeechRecognizerControl();
                case AzureKinectBodyTrackerConfiguration _:
                    return new Configuration.AzureKinectBodyTrackerControl();
                case Mpeg4WriterConfiguration _:
                    return new Configuration.Mpeg4WriterControl();
                case WaveFileWriterConfiguration _:
                    return new Configuration.WaveFileWriterControl();
                case AudioPlayerConfiguration _:
                    return new Configuration.AudioPlayerControl();
                case ImageEncoderConfiguration _:
                    return new Configuration.ImageEncoderControl();
                #endregion

                #region OpenSense
                case FlipColorVideoConfiguration _:
                    return new Configuration.VideoFrameFlipControl();
                case GazeToDisplayConfiguration _:
                    return new Configuration.GazeToDisplayControl();
                case OpenPoseConfiguration _:
                    return new Configuration.OpenPoseControl();
                case OpenFaceConfiguration _:
                    return new Configuration.OpenFaceControl();
                case OpenSmileConfiguration _:
                    return new Configuration.OpenSmileControl();
                case EmotionDetectorConfiguration _:
                    return new Configuration.EmotionDetectorControl();
                #endregion

                default:
                    return new Configuration.DefaultControl();
            }
        }

        public static UserControl CreateVisualizerControl(InstanceEnvironment env) {
            switch (env.Instance) {
                case Components.Display.BooleanVisualizer _:
                    return new Display.BooleanVisualizerControl();
                case Components.Display.ColorVideoVisualizer _:
                case Components.Display.OpenFaceVisualizer _:
                case Components.Display.OpenPoseVisualizer _:
                case Components.Display.AzureKinectBodyTrackerVisualizer _:
                    return new Display.ColorVideoVisualizerControl();
                case Components.Display.DepthVideoVisualizer _:
                    return new Display.DepthVideoVisualizerControl();
                case Components.Display.HeadGestureVisualizer _:
                    return new Display.HeadGestureVisualizerControl();
                case Components.Display.GazeToDisplayVisualizer _:
                    return new Display.GazeToDisplayVisualizerControl();
                case Components.Display.EmotionVisualizer _:
                    return new Display.EmotionVisualizerControl();

                case Components.GazeToDisplay _:
                    return new Display.GazeToDisplayControl();
                case Components.OpenPose.OpenPose _:
                    return new Display.OpenPoseControl();
                case Components.OpenFace.OpenFace _:
                    return new Display.OpenFaceControl();
                case Components.OpenSmile.OpenSmile _:
                    return new Display.OpenSmileControl();

                case Components.Display.AudioVisualizer _:
                case Components.Display.BiopacVisualizer _:
                    return new Display.AudioVisualizerControl();
                case Components.Display.StreamingSpeechRecognitionVisualizer _:
                    return new Display.StreamingSpeechRecognitionVisualizerControl();
                case Components.Display.OpenSmileVisualizer _:
                    return new Display.OpenSmileVisualizerControl();
                default:
                    return null;
            }
        }
    }
}
