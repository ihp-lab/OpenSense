# openSMILE for signal processing

## Description

This sample contains a pipeline for demonstrating the usage of openSMILE component.

## How to use

+ Open the `sample.pipe.json` configuration file using a `Pipeline Editor` window.
+ Select the `Microsoft.Psi.Audio.AudioCapture` instance and set a microphone.
+ Select the `OpenSense.Component.OpenSmile.OpenSmile` instance and fill in the missing part of the `Configuration file path` option.
+ Run the pipeline.
+ Select `pcm_LOGenergy` under `Select a feature`, then you can see the result.

## Steps to replicate

+ Change the pipeline default `Delivery policy` on the top to `Unlimited`.
+ Add a `Microsoft.Psi.Audio.AudioCapture` instance.
+ Add a `OpenSense.Component.OpenSmile.OpenSmile` instance.
+ Add the `In` input and connect it to the `Out` output of the `AudioCapture` instance.
+ Use `waveSource` as the input port index. This name is defined inside the openSMILE configuration file, you cannot change it here.
+ Add a `OpenSense.Component.OpenSmile.Visualizer.OpenSmileVisualizer` instance.
+ Add the `In` input and connect it to the `Out` output of the `OpenSmile` instance.
+ Use `dataSink` as the output port index. This name is also defined inside the openSMILE configuration file.
+ Save the pipeline configuration.

## Tips

+ openSMILE has its own configuration file format. Please reference to its own documents to understand the format.
+ The openSMILE in our project is a customized version with 2 additional openSMILE components, 1 for receiving wave data and 1 for sending back results.
+ The `sample1/opensmile_energy.conf` is referencing `sample1/shared/common_io.conf`. This `common_io.conf` is the interface for exchanging data between OpenSense and openSMILE.
+ There is a `OpenSmile Configuration Converter` widget to converting a regular openSMILE configuration file to a OpenSense-compatible openSMILE configuration. This is down by simply replacing the wave source and data sink segments. This widget may not always work.
+ `sample2/opensmile_emobase_live4.conf` is a sample openSMILE configuration processed by `OpenSmile Configuration Converter`.
