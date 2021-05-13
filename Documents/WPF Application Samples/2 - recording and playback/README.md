# Recording and playback

## Description

This sample contains a pipeline for recording video & audio to a local path, and a pipeline for playback.

## How to use

### Recording

+ Open `recording.pipe.json` configuration file using a `Pipeline Editor` window.
+ Click the `Microsoft.Psi.Media.MediaCapture` instance, and select the camera you want to use.
+ Click the `Psi Store Exporter` instance, set `Root path` to the path you want to put the recorded data.
+ Execute the pipeline for a while, then stop the pipeline.

### Playback

+ Open `playback.pipe.json` configuration file  using a `Pipeline Editor` window.
+ Click the `Psi Store Importer` instance, set `Root path` to the path you set before.
+ Execute the pipeline, you will see recorded video and audio.

## Steps to replicate

### Recording configuration

+ Add a `Microsoft.Psi.Media.MediaCapture` instance.
+ Select the `Capture audio` option.
+ Add a `Psi Store Exporter` instance.
+ Set the `Store name` field, such as `sample`. A non-empty store name is required.
+ Set a `Root path`.
+ Add an `In` input and connect it to the `Video` output of the `MediaCapture` instance.
+ Give this video stream a name, such as `video`. The `In` input of `Psi Store Exporter` is dictionary type, so you can add multiple concrete inputs inside.
+ Add another `In` input and connect it to the `Audio` output of the `MediaCapture` instance.
+ Give this audio stream a name, such as `audio`.
+ Check both checkboxes on the right of `Large message` option. This option is not mandatory, it is only for optimization purposes.
+ Save this pipeline configuration.

### Playback configuration

+ Add a `Psi Store Importer` instance.
+ Set the `Store name` and `Root path` options.
+ Add a `OpenSense.Component.Imaging.Visualizer.ColorVideoVisualizer` isntance to visualize recorded video.
+ Add the `In` input and connect it to the `Out` output of `Psi Store Importer` instance.
+ Fill-in the video stream name to select a concrete output.
+ Add a `OpenSense.Component.Audio.Visualizer.AudioVisualizer` instance.
+ Add the `In` input and connect it to the `Out` output of `Psi Store Importer` instance.
+ Fill-in the audio stream name.
+ Save this pipeline configuration.

## Tips

+ The recorded binary data can also be opened by [Psi Studio](https://github.com/microsoft/psi/wiki/Psi-Studio).
+ With the default setting, you will see error message if you want to execute the recording pipeline the second time. You can delete recorded files or change to another path to reset. To enable multiple executions without changing the configuration, we recommand you select the `Create subdirectory` option. This option will help creating a unique subdirectory for each execution. If you selected this option, the playback path should be adjusted to one of the subdirectories.
+ For aggregated input/output ports, indexers are required to sepcify concrete input/output ports. For list type based aggregated ports, indexers are 0-based numeric indexes. For dictionary type based agregated ports, indexers are strings.
