#pragma once

#include <core/dataMemory.hpp>
#include <core/dataWriter.hpp>
#include <core/dataSource.hpp>
#include <core/dataReader.hpp>
#include <core/dataSink.hpp>
#include <core/dataProcessor.hpp>
#include <core/winToVecProcessor.hpp>
#include <core/vectorProcessor.hpp>
#include <dspcore/framer.hpp>
#include <lldcore/energy.hpp>

#include "Common.h"
#include "Configuration.h"

using namespace System;
using namespace System::Collections::Generic;

#define CommaCharacter ,

#define DefineField_Full(name, type, dlft, description, isMandatory)\
protected:\
	static type name ## _Default = dlft;\
	type name ## _Value = dlft;\
	bool name ## _IsSet_Value = false;\
	bool name ## _Enabled_Value = true;\
public:\
	[ConfigTypeField(description, IsMandatory = isMandatory)]\
	property type name {\
		type get() {\
			return name ## _Value;\
		}\
		void set(type value) {\
			name ## _Value = value;\
			name ## _IsSet_Value = true;\
		}\
	}\
	property bool name ## _IsSet {\
		bool get() {\
			return name ## _IsSet_Value;\
		}\
	}\
	property bool name ## _Enabled {\
		bool get() {\
			return name ## _Enabled_Value;\
		}\
	}\

#define DefineField_Mandatory(name, type, dflt, description) DefineField_Full(name, type, dflt, description, true)

#define DefineField(name, type, dflt, description) DefineField_Full(name, type, dflt, description, false)

#define SetFieldDefault(name) name = name ## _Default;

#define ChangeFieldDefault(name, value)\
name ## _Default = value;\
name ## _Value = value;

#define DisableField(name) name ## _Enabled_Value = false;

#define CheckWhetherNeedToSet(name) name ## _Enabled && name ## _IsSet

#define SetInstance_Full(before, setter, prop, keySuffix, value, after) {\
	before;\
	const auto fullKey = (fieldPrefix != nullptr ? fieldPrefix + "." : String::Empty) + #prop + keySuffix;\
	const auto fullKeyCharArray = Utility::ConvertStringToCharArray(fullKey);\
	instance->setter(fullKeyCharArray, value);\
	delete[] fullKeyCharArray;\
	after;\
}

#define SetInstance_PlainProperty(before, setter, prop, value, after) if (CheckWhetherNeedToSet(prop)) SetInstance_Full(before, setter, prop, String::Empty, value, after);

#define SetInstance_String(prop) SetInstance_PlainProperty(auto cStyleStr = Utility::ConvertStringToCharArray(prop), setStr, prop, cStyleStr, delete[] cStyleStr);

#define SetInstance_ValueType(setter, prop) SetInstance_PlainProperty(, setter, prop, prop, );

#define SetInstance_Int(prop) SetInstance_ValueType(setInt, prop);

#define SetInstance_Bool(prop) SetInstance_Int(prop);

#define SetInstance_Double(prop) SetInstance_ValueType(setDouble, prop);

#define SetInstance_Char(prop) SetInstance_ValueType(setChar, prop);

/* this set method does not working!
#define SetInstance_StringArray(prop) SetInstance_PlainProperty(\
		auto cv = new ConfigValueStrArr(prop->Count);\
		for (auto i = 0; i < prop->Count; i++) {\
			auto str = Utility::ConvertStringToCharArray(prop[i]);\
			cv->setValue(str, i);\
			delete[] str;\
		}\
	, setValue, prop, String::Empty, cv, );
*/

#define SetInstance_StringArray(prop) \
if (prop ## _Enabled && prop != nullptr) {\
	for (auto i = 0; i < prop->Count; i++) {\
		SetInstance_Full(auto valueCharArray = Utility::ConvertStringToCharArray(prop[i]);, setStr, prop, "[" + i + "]", valueCharArray, delete[] valueCharArray;);\
	}\
}

#define SetInstance_Object(prop) \
if (prop ## _Enabled && prop != nullptr) {\
	prop->SetInstanceValuesWithFullFieldName((fieldPrefix != nullptr ? fieldPrefix + "." : String::Empty) + #prop, instance, cman);\
}

namespace OpenSmileInterop {

	[AttributeUsage(AttributeTargets::Property)]
	ref struct ConfigTypeField : public Attribute {
	protected:
		String^ description = nullptr;
		bool isMandatory = false;

	public:
		property String^ Description {
			String^ get() {
				return description;
			}

			void set(String^ value) {
				description = value;
			}
		}

		property bool IsMandatory {
			bool get() {
				return isMandatory;
			}

			void set(bool value) {
				isMandatory = value;
			}
		}

		ConfigTypeField() {}

		ConfigTypeField(String^ description) {
			this->description = description;
		}
	};

	public ref class ComponentManagerInstInstanceConfiguration : public SmileComponentInstanceConfiguration {
		DefineField(type, String^, nullptr, "");
		DefineField(threadId, int, -1, "");

	public:
		property String^ ComponentTypeName {
			virtual String^ get() override {
				return "cComponentManagerInst";
			}
		}

		ComponentManagerInstInstanceConfiguration() {
			componentType = nullptr;
		}

		virtual void SetInstanceValuesWithFullFieldName(String^ fieldPrefix, ConfigInstance* instance, cConfigManager *cman) override {
			SmileComponentInstanceConfiguration::SetInstanceValuesWithFullFieldName(fieldPrefix, instance, cman);
			SetInstance_String(type);
			SetInstance_Int(threadId);
		}
	};

	public ref class ComponentManagerInstanceConfiguration : public SmileComponentInstanceConfiguration {
		DefineField(printLevelStats, int, 1, "");
		DefineField(profiling, bool, false, "");
		DefineField(nThreads, int, 1, "");
		DefineField(threadPriority, int, 0, "");
		DefineField(execDebug, bool, false, "");
		DefineField(oldSingleIterationTickLoop, bool, false , "");

	public:
		List<Tuple<String^, ComponentManagerInstInstanceConfiguration^>^>^ instance = gcnew List<Tuple<String^, ComponentManagerInstInstanceConfiguration^>^> ();

		ComponentManagerInstanceConfiguration() {
			componentType = cComponentManager::typeid;
		}

		virtual void SetInstanceValuesWithFullFieldName(String^ fieldPrefix, ConfigInstance* instance, cConfigManager *cman) override {
			SmileComponentInstanceConfiguration::SetInstanceValuesWithFullFieldName(fieldPrefix, instance, cman);
			if (this->instance != nullptr) {
				for each (auto inst in this->instance) {
					auto nextPrefix = (fieldPrefix != nullptr ? fieldPrefix + "." : String::Empty) + "instance" + "[" + inst->Item1 + "]";
					inst->Item2->SetInstanceValuesWithFullFieldName(nextPrefix, instance, cman);
				}
			}
			SetInstance_Int(printLevelStats);
			SetInstance_Bool(profiling);
			SetInstance_Int(nThreads);
			SetInstance_Int(threadPriority);
			SetInstance_Bool(execDebug);
			SetInstance_Bool(oldSingleIterationTickLoop);
		}
	};

	public ref class DataMemoryLevelInstanceConfiguration : public SmileComponentInstanceConfiguration {
		DefineField(name, String^, nullptr, "");
		DefineField(type, String^, "float", "");
		DefineField(isRb, bool, true, "");
		DefineField(nT, int, 100, "");
		DefineField(T, double, 0.0, "");
		DefineField(lenSec, double, 0.0, "");
		DefineField(frameSizeSec, double, 0.0, "");
		DefineField(growDyn, bool, false, "");
		DefineField(noHang, bool, true, "");
	public:

		DataMemoryLevelInstanceConfiguration() {
			componentType = cDataMemoryLevel::typeid;
		}

		virtual void SetInstanceValuesWithFullFieldName(String^ fieldPrefix, ConfigInstance* instance, cConfigManager *cman) override {
			SmileComponentInstanceConfiguration::SetInstanceValuesWithFullFieldName(fieldPrefix, instance, cman);
			SetInstance_String(name);
			SetInstance_String(type);
			SetInstance_Bool(isRb);
			SetInstance_Int(nT);
			SetInstance_Double(T);
			SetInstance_Double(lenSec);
			SetInstance_Double(frameSizeSec);
			SetInstance_Bool(growDyn);
			SetInstance_Bool(noHang);
		}
	};

	public ref class DataMemoryInstanceConfiguration : public SmileComponentInstanceConfiguration {
		DefineField(isRb, int, 1, "");
		DefineField(nT, int, 100, "");
	public:
		DataMemoryInstanceConfiguration() {
			componentType = cDataMemory::typeid;
		}

		virtual void SetInstanceValuesWithFullFieldName(String^ fieldPrefix, ConfigInstance* instance, cConfigManager *cman) override {
			SmileComponentInstanceConfiguration::SetInstanceValuesWithFullFieldName(fieldPrefix, instance, cman);
			SetInstance_Int(isRb);
			SetInstance_Int(nT);
		}
	};

	public ref class DataWriterInstanceConfiguration : public SmileComponentInstanceConfiguration {
		DefineField(dmInstance, String^, "dataMemory", "");
		DefineField_Mandatory(dmLevel, String^, nullptr, "");
		DefineField(levelconf, DataMemoryLevelInstanceConfiguration^, gcnew DataMemoryLevelInstanceConfiguration, "");
	public:

		DataWriterInstanceConfiguration() {
			componentType = cDataWriter::typeid;
		}

		virtual void SetInstanceValuesWithFullFieldName(String^ fieldPrefix, ConfigInstance* instance, cConfigManager *cman) override {
			SmileComponentInstanceConfiguration::SetInstanceValuesWithFullFieldName(fieldPrefix, instance, cman);
			SetInstance_String(dmInstance);
			SetInstance_String(dmLevel);
			SetInstance_Object(levelconf);
		}
	};

	public ref class DataSourceInstanceConfiguration : public SmileComponentInstanceConfiguration {
		DefineField(writer, DataWriterInstanceConfiguration^, gcnew DataWriterInstanceConfiguration, "");
		DefineField(buffersize, int, 0, "");
		DefineField(buffersize_sec, double, 0.0, "");
		DefineField(blocksize, int, 0, "");
		DefineField(blocksizeW, int, 0, "");
		DefineField(blocksize_sec, double, 0.0, "");
		DefineField(blocksizeW_sec, double, 0.0, "");
		DefineField(period, double, 0.0, "");
		DefineField(basePeriod, double, 0.0, "");
	public:

		DataSourceInstanceConfiguration() {
			componentType = cDataSource::typeid;
		}

		virtual void SetInstanceValuesWithFullFieldName(String^ fieldPrefix, ConfigInstance* instance, cConfigManager *cman) override {
			SmileComponentInstanceConfiguration::SetInstanceValuesWithFullFieldName(fieldPrefix, instance, cman);
			SetInstance_Object(writer);
			SetInstance_Int(buffersize);
			SetInstance_Double(buffersize_sec);
			SetInstance_Int(blocksize);
			SetInstance_Int(blocksizeW);
			SetInstance_Double(blocksize_sec);
			SetInstance_Double(blocksizeW_sec);
			SetInstance_Double(period);
			SetInstance_Double(basePeriod);
		}
	};

	public ref class DataReaderInstanceConfiguration : public SmileComponentInstanceConfiguration {
		DefineField(dmInstance, String^, "dataMemory", "");
		DefineField_Mandatory(dmLevel, List<String^>^, gcnew List<String^>, "");
		DefineField(forceAsyncMerge, bool, false, "");
		DefineField(errorOnFullInputIncomplete, bool, true, "");
	public:

		DataReaderInstanceConfiguration() {
			componentType = cDataReader::typeid;
		}

		virtual void SetInstanceValuesWithFullFieldName(String^ fieldPrefix, ConfigInstance* instance, cConfigManager *cman) override {
			SmileComponentInstanceConfiguration::SetInstanceValuesWithFullFieldName(fieldPrefix, instance, cman);
			SetInstance_String(dmInstance);
			SetInstance_StringArray(dmLevel);
			SetInstance_Bool(forceAsyncMerge);
			SetInstance_Bool(errorOnFullInputIncomplete);
		}
	};

	public ref class DataSinkInstanceConfiguration : public SmileComponentInstanceConfiguration {
		DefineField(reader, DataReaderInstanceConfiguration^, gcnew DataReaderInstanceConfiguration, "");
		DefineField(blocksize, int, 0, "");
		DefineField(blocksizeR, int, 0, "");
		DefineField(blocksize_sec, double, 0.0, "");
		DefineField(blocksizeR_sec, double, 0.0, "");
		DefineField(errorOnNoOutput, bool, false, "");
	public:

		DataSinkInstanceConfiguration() {
			componentType = cDataSink::typeid;
		}

		virtual void SetInstanceValuesWithFullFieldName(String^ fieldPrefix, ConfigInstance* instance, cConfigManager *cman) override {
			SmileComponentInstanceConfiguration::SetInstanceValuesWithFullFieldName(fieldPrefix, instance, cman);
			SetInstance_Object(reader);
			SetInstance_Int(blocksize);
			SetInstance_Int(blocksizeR);
			SetInstance_Double(blocksize_sec);
			SetInstance_Double(blocksizeR_sec);
			SetInstance_Bool(errorOnNoOutput);
		}
	};

	public ref class DataProcessorInstanceConfiguration : public SmileComponentInstanceConfiguration {
		DefineField(reader, DataReaderInstanceConfiguration^, gcnew DataReaderInstanceConfiguration, "");
		DefineField(writer, DataWriterInstanceConfiguration^, gcnew DataWriterInstanceConfiguration, "");
		DefineField(buffersize, int, 0, "");
		DefineField(buffersize_sec, double, 0.0, "");
		DefineField(blocksize, int, 0, "");
		DefineField(blocksizeR, int, 0, "");
		DefineField(blocksizeW, int, 0, "");
		DefineField(blocksize_sec, double, 0.0, "");
		DefineField(blocksizeR_sec, double, 0.0, "");
		DefineField(blocksizeW_sec, double, 0.0, "");
		DefineField(nameAppend, String^, nullptr, "");
		DefineField(copyInputName, bool, true, "");
		DefineField(EOIlevel, int, 0, "");

	public:

		DataProcessorInstanceConfiguration() {
			componentType = cDataProcessor::typeid;
		}

		virtual void SetInstanceValuesWithFullFieldName(String^ fieldPrefix, ConfigInstance* instance, cConfigManager *cman) override {
			SmileComponentInstanceConfiguration::SetInstanceValuesWithFullFieldName(fieldPrefix, instance, cman);
			SetInstance_Object(reader);
			SetInstance_Object(writer);
			SetInstance_Int(buffersize);
			SetInstance_Double(buffersize_sec);
			SetInstance_Int(blocksize);
			SetInstance_Int(blocksizeR);
			SetInstance_Int(blocksizeW);
			SetInstance_Double(blocksize_sec);
			SetInstance_Double(blocksizeR_sec);
			SetInstance_Double(blocksizeW_sec);
			SetInstance_String(nameAppend);
			SetInstance_Bool(copyInputName);
			SetInstance_Int(EOIlevel);
		}
	};

	public ref class WinToVecProcessorInstanceConfiguration : public DataProcessorInstanceConfiguration {
		DefineField(allowLastFrameIncomplete, bool, false, "");
		DefineField(frameMode, String^, "fixed", "");
		DefineField(frameListFile, String^, nullptr, "");
		DefineField(frameList, String^, nullptr, "");
		DefineField(frameSize, double, 0.025, "");
		DefineField(frameStep, double, 0.0, "");
		DefineField(frameSizeFrames, int, 0, "");
		DefineField(frameStepFrames, int, 0, "");
		DefineField(frameCenter, int, 0, "");// why not double?
		DefineField(frameCenterFrames, int, 0, "");
		DefineField(frameCenterSpecial, String^, "left", "");
		DefineField(noPostEOIprocessing, bool, true, "");

	public:

		WinToVecProcessorInstanceConfiguration(){
			componentType = cWinToVecProcessor::typeid;
			// disable
			DisableField(blocksize);
			DisableField(blocksizeR);
			DisableField(blocksizeW);
			DisableField(blocksize);
			DisableField(blocksizeR);
			DisableField(blocksizeW);
		}

		virtual void SetInstanceValuesWithFullFieldName(String^ fieldPrefix, ConfigInstance* instance, cConfigManager *cman) override {
			DataProcessorInstanceConfiguration::SetInstanceValuesWithFullFieldName(fieldPrefix, instance, cman);
			SetInstance_Bool(allowLastFrameIncomplete);
			SetInstance_String(frameMode);
			SetInstance_String(frameListFile);
			SetInstance_String(frameList);
			SetInstance_Double(frameSize);
			SetInstance_Double(frameStep);
			SetInstance_Int(frameSizeFrames);
			SetInstance_Int(frameStepFrames);
			SetInstance_Int(frameCenter);
			SetInstance_Int(frameCenterFrames);
			SetInstance_String(frameCenterSpecial);
			SetInstance_Bool(noPostEOIprocessing);
		}
	};

	public ref class VectorProcessorInstanceConfiguration : public DataProcessorInstanceConfiguration {
		DefineField(processArrayFields, bool, true, "");
		DefineField(includeSingleElementFields, bool, false, "");
		DefineField(preserveFieldNames, bool, true, "");

	public:

		VectorProcessorInstanceConfiguration() {
			componentType = cVectorProcessor::typeid;
		}

		virtual void SetInstanceValuesWithFullFieldName(String^ fieldPrefix, ConfigInstance* instance, cConfigManager *cman) override {
			DataProcessorInstanceConfiguration::SetInstanceValuesWithFullFieldName(fieldPrefix, instance, cman);
			SetInstance_Bool(processArrayFields);
			SetInstance_Bool(includeSingleElementFields);
			SetInstance_Bool(preserveFieldNames);
		}
	};

	public ref class FramerInstanceConfiguration : public WinToVecProcessorInstanceConfiguration {
	public:

		FramerInstanceConfiguration() {
			InstanceName = "framer";
			componentType = cFramer::typeid;
		}

		virtual void SetInstanceValuesWithFullFieldName(String^ fieldPrefix, ConfigInstance* instance, cConfigManager *cman) override {
			WinToVecProcessorInstanceConfiguration::SetInstanceValuesWithFullFieldName(fieldPrefix, instance, cman);
		}
	};

	public ref class EnergyInstanceConfiguration : public VectorProcessorInstanceConfiguration {
		DefineField(htkcompatible, bool, false, "");
		DefineField(rms, bool, true, "");
		DefineField(energy2, bool, false, "");
		DefineField(log, bool, true, "");
		DefineField(escaleLog, double, 1.0, "");
		DefineField(escaleRms, double, 1.0, "");
		DefineField(escaleSquare, double, 1.0, "");
		DefineField(ebiasLog, double, 0.0, "");
		DefineField(ebiasRms, double, 0.0, "");
		DefineField(ebiasSquare, double, 0.0, "");

	public:

		EnergyInstanceConfiguration() {
			InstanceName = "energy";
			componentType = cEnergy::typeid;
			// overwrite
			ChangeFieldDefault(nameAppend, "energy");
			ChangeFieldDefault(processArrayFields, false);
		}

		virtual void SetInstanceValuesWithFullFieldName(String^ fieldPrefix, ConfigInstance* instance, cConfigManager *cman) override {
			VectorProcessorInstanceConfiguration::SetInstanceValuesWithFullFieldName(fieldPrefix, instance, cman);
			SetInstance_Bool(htkcompatible);
			SetInstance_Bool(rms);
			SetInstance_Bool(energy2);
			SetInstance_Bool(log);
			SetInstance_Double(escaleLog);
			SetInstance_Double(escaleRms);
			SetInstance_Double(escaleSquare);
			SetInstance_Double(ebiasLog);
			SetInstance_Double(ebiasRms);
			SetInstance_Double(ebiasSquare);
		}
	};
}