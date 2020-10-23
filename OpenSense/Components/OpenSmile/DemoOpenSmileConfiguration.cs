using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenSmileInterop;

namespace OpenSense.Components.OpenSmile {
    public class DemoOpenSmileConfiguration : Configuration {

        public DemoOpenSmileConfiguration() {
            ConfigurationFilename = "./opensmile_emobase_live4.conf";

            /* This is a energy extract demoe for not using config file
            UseConfigFile = false;
			var waveSource = new RawWaveSourceInstanceConfiguration();
			waveSource.writer.dmLevel = "wave";
			waveSource.monoMixdown = true;
			waveSource.sampleRate = 16000;
			waveSource.channels = 2;
			waveSource.blocksize_sec = 0.01;
			
			var framer = new FramerInstanceConfiguration();
			framer.reader.dmLevel.Add(waveSource.writer.dmLevel);
			framer.writer.dmLevel = "waveframes";
			framer.copyInputName = true;
			framer.frameMode = "fixed";
			framer.frameSize = 0.025;
			framer.frameStep = 0.01;
			framer.frameCenterSpecial = "left";
			framer.noPostEOIprocessing = true;

			var energy = new EnergyInstanceConfiguration();
			energy.reader.dmLevel.Add(framer.writer.dmLevel);
			energy.writer.dmLevel = "energy";
			energy.nameAppend = "energy";
			energy.copyInputName = true;
			energy.processArrayFields = false;
			energy.htkcompatible = false;
			energy.rms = false;
			energy.log = true;

			var dataSink = new RawDataSinkInstanceConfiguration();
			dataSink.reader.dmLevel.Add(energy.writer.dmLevel);
			dataSink.fieldInfo = true;
			
			InstanceConfigurations.Add(waveSource);
			InstanceConfigurations.Add(framer);
			InstanceConfigurations.Add(energy);
			InstanceConfigurations.Add(dataSink);
			*/
        }
    }
}
