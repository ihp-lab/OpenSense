# Speech recognition

## Description

This sample contains a pipeline for speech recognition using an online service provided by Microsoft Azure.
This sample shows the usage of `Delivery policy` and `Join Operator`.

## How to use

+ Open the `sample.pipe.json` configuration file using a `Pipeline Editor` window.
+ Select the `Microsoft.Psi.Audio.AudioCapture` instance and set a microphone.
+ Select the `Microsoft.Psi.CognitiveServices.Speech.AzureSpeechRecognizer` instance and fill in your subscription key and region got from Microsoft Azure.
+ Run the pipeline.

## Steps to replicate

+ Change the pipeline default `Delivery policy` on the top to `Unlimited`. With the default `Latest message`, the delay of speech recognizers will cause audio frames are dropped and you will not receive meaningful results.
+ Add a `Microsoft.Psi.Audio.AudioCapture` instance.
+ Add a `Microsoft.Psi.Speech.SystemVoiceActivityDetector` instance. Voice activity detection results are required for all web-based speech recognizers.
+ Add the `In` input and connect it to the `Out` output of the `AudioCapture` instance.
+ Add a `Join Operator` instance.
+ Add the `Primary` input and connect it to the `Out` output of the `AudioCapture` instance.
+ Add the `Secondary` input and connect it to the `Out` output of the `SystemVoiceActivityDetector` instance.
+ Add a `Microsoft.Psi.CognitiveServices.Speech.AzureSpeechRecognizer` instance.
+ Add the `In` input and connect it to the `Out` output of the `Join Operator` instance.
+ Add an `OpenSense.Component.Psi.Speech.Visualizer.StreamingSpeechRecognitionVisualizer` instance.
+ Add the `In` input and connect it to the `Out` output of the `AzureSpeechRecognizer` instance.
+ Save the pipeline configuration.

## Tips

+ `Microsoft.Psi.Speech.SystemVoiceActivityDetector` is only available in .net framework, it is removed in .net core and no substitution is provided. This can be a problem if you want to use speech recognition in other platforms. Microsoft /psi has a [sample](https://github.com/Microsoft/psi-samples/tree/main/Samples/SimpleVoiceActivityDetector) for making a simple voice activity detector which we plan to add to our project.
+ For complex pipelines, you may want to set pipeline default delivery policy to some value other than `Unlimited`. To ensure correct speech recognition results, all data connections up from audio capturers down to speech recognizers should have local delivery policy set to `Unlimited` or other reasonable values.
+ Other speech recognizers are also available in OpenSense.
+ Many components have a `Input format` option for their audio inputs, we haven't found any use-case that we need to match this option to the real input format.
