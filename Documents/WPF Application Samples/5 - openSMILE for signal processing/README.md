# openSMILE for signal processing

## Description

This sample contains a pipeline for demonstrating the usage of openSMILE component.

## How to use

+ Open the `sample.pipe.json` configuration file using a `Pipeline Editor` window.
+ Select the `Audio Capture` instance and set a microphone.
+ Select the `openSMILE` instance and fill in the missing part of the `Configuration File` option.
+ Run the pipeline.
+ Select `pcm_LOGenergy` in the `openSMILE Visualizer` instance, then you can see the result.

## Steps to replicate

+ Change the pipeline default `Delivery policy` on the top to `Unlimited`.
+ Add a `Audio Capture` instance.
+ Add a `openSMILE` instance.
+ Add the `In` input and connect it to the `Out` output of the `AudioCapture` instance.
+ Use `waveSource` as the input port index. This name is defined inside the openSMILE configuration file, you cannot change it here.
+ Add a `openSMILE Visualizer` instance.
+ Add the `In` input and connect it to the `Out` output of the `OpenSmile` instance.
+ Use `dataSink` as the output port index. This name is also defined inside the openSMILE configuration file.
+ Add the `FeatureNamesIn` input and connect it to the `FeatureNamesOut` output of the `OpenSmile` instance, also with `dataSink` as the output port index. Without it, the visualizer lists features by index.
+ Save the pipeline configuration.

## Tips

+ openSMILE has its own configuration file format. Please reference to its own documents to understand the format.
+ OpenSense sends audio to openSMILE through a `cExternalAudioSource` component, and receives results through a `cExternalSink` component, both declared in the openSMILE configuration file.
+ The `sample1/opensmile_energy.conf` is referencing `sample1/shared/common_io.conf`. This `common_io.conf` is the interface for exchanging data between OpenSense and openSMILE.
+ The audio format in the openSMILE configuration file must match the incoming audio, otherwise the pipeline stops with an error that explains the difference. Both samples expect 16 kHz, mono, 16-bit audio, which is what `Audio Capture` produces by default. Audio in other formats can be converted with an `Audio Resampler`.
+ To use another openSMILE configuration file, replace its audio input and result output parts with `cExternalAudioSource` and `cExternalSink`.
+ `sample2/opensmile_emobase_live4.conf` is the `emobase_live4.conf` of openSMILE adapted this way, and changed to keep working after long pauses in the audio.
