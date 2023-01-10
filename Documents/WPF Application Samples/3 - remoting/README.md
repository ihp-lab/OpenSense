# Remoting

## Description

This sample contains a pipeline for sending video to remote client pipelines and a pipeline for receiving that data from the other pipeline.

## How to use

+ Open `host.pipe.json` configuration file using a `Pipeline Runner` (not `Pipeline Editor`) window.
+ Open `client.pipe.json` configuration file using another `Pipeline Runner` window.
+ Run the host pipeline first, then run the client pipeline. If the client pipeline is started first, the application will block for establishing a connection, the UI will freeze. You can run the client pipeline first if you open two sample pipelines in separate OpenSense WPF application instances, the connection timeout is set to 60 seconds in the sample client configuration.

## Steps to replicate

### Host configuration

+ Add a `Media Capture` instance.
+ Add a `Remote Exporter` instance.
+ Add an `In` input and connect it to the `Video` output of the `MediaCapture` instance.
+ Give this video stream a name, such as `video`.
+ Check the `Large message` checkbox for the video stream. For video streams, this option is recommended.
+ Save this pipeline configuration.

### Client configuration

+ Add a `Remote Importer` instance.
+ Change the `Connection timeout seconds` option as needed.
+ Add a `Color Image Visualizer` instance to visualize the received video stream.
+ Add the `In` input and connect it to the `Out` output of `Remote Importer` instance.
+ Fill-in the video stream name to select the stream.
+ Save this pipeline configuration.

## Tips

+ The application will crash when you run the host pipeline the second time using one OpenSense WPF application instance. We think this is caused by the TCP/UDP ports are not correctly released by /psi. You have to re-launch an OpenSense WPF application to run the host pipeline again.
+ Use a lower resolution to reduce the delay. Sending 4K video streams can be hard.
