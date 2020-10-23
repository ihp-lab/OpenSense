# Function

A wrapper of [openSMILE](https://github.com/naxingyu/opensmile), which can perform audio feature extraction.

# Hardware

# Class

`OpenSense.Components.OpenSmileComponent`

# Input

`Microsoft.Psi.Audio.AudioBuffer`

# Output

`OpenSense.Components.OpenSmileVector`

Wrapper of openSMILE raw vector output. May consist multiple output fields which can be configured in configurations.

# Configuration

The openSMILE has its own pipeline and components, a `openSmileInterop.Configuration` instance is required in OpenSmileComponent's constructor to setup underlying openSMILE back-end.
The openSMILE back-end can be configured using codes or a openSMILE configuration file.
If a configuration file is used, openSMILE pipeline configuration using codes will be ignored.

## Source and Sink

To adapt OpenSense, a wave source component named `cRawWaveSource` and a raw data sink component named `cRawDataSink` are added into openSMILE to redirect inputs and outputs.

Multiple inputs and outputs are supported.
Input (RawWaveSource) or output (RawDataSink) of OpenSmileComponent can be accessed by OpenSmileComponent's `In[name]` or `Out[name]` indexers indexed by input/output openSMILE component instance name assigned in the configuration.

If there are multiple inputs (RawWaveSource), one of them should be assigned as the main input to synchronize multiple inputs.
So that, only when a new audio frame for this designated main input source arrives, the openSMILE back-end will be executed once.
The default main input source is the last `RawWaveSource` instance in the configuration.
To designate a input source as the main input source, assign the `SyncInputName` field of the `OpenSmileComponent` instance using input's name.

## File

When using a configuration file, in the `Configuration` instance, the `UseConfigFile` field need to be set to `true` and the `ConfigFilename` field need to be assigned to the configuration file's path.

For detail of openSMILE configuration file format, please refer to [openSMILE config file documentation](https://github.com/naxingyu/opensmile/blob/master/doc/configfile.txt).
Some sample configuration files can be found in [openSMILE repository](https://github.com/naxingyu/opensmile/tree/master/config).

`cRawWaveSource` and `cRawDataSink` needed to be added into configuration files to receive or emit data.

## Code

To configure openSMILE back-end using codes, first instantiate individual openSMILE components' configuration `openSmileInterop.*Configuration` (such as RawWaveSourceInstanceConfiguration, FramerInstanceConfiguration, RawDataSinkInstanceConfiguration) classes, then add these configuration instances into `openSmileInterop.Configuration`'s `InstanceConfigurations` field.
*Note: These openSMILE component configuration classes are simple wrapper of existing component configurations and they are added by hand in [openSmileInterop](https://github.com/intelligent-human-perception-laboratory/openSmileInterop) project, thus some of openSMILE components may not be covered.*