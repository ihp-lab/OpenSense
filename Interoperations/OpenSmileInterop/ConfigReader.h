#pragma once

#include "gcroot.h"

#include <core/configManager.hpp>

#include "Common.h"
#include "Configuration.h"
#include "SmileComponentConfiguration.h"

using namespace System;
using OpenSmileInterop::InstanceConfiguration;

#define MODULE "configManager"

class cInteropConfigReader : public cConfigReader {
private:
	gcroot<List<InstanceConfiguration^>^> configurations;
public:


	cInteropConfigReader(List<InstanceConfiguration^>^ _configurations, int id = -1, cCommandlineParser *cmdparser_ = nullptr) : cConfigReader(nullptr, id, cmdparser_) {
		if (_configurations == nullptr) {
			throw gcnew ArgumentNullException("_configurations");
		}

		List<InstanceConfiguration^>^ configs = _configurations; //unboxing
		bool hasConfigManagerConfiguration = false;
		for each (auto c in configs) {
			if (c->ComponentTypeName == cComponentManager::typeid->Name || c->InstanceName == "componentInstances") {
				hasConfigManagerConfiguration = true;
				break;
			}
		}
		if (!hasConfigManagerConfiguration) {
			auto configManagerConfig = gcnew OpenSmileInterop::ComponentManagerInstanceConfiguration();//TODO: if you want to further configurate ComponentManagerInstanceConfiguration by yourself, please rearrange this function.
			configManagerConfig->InstanceName = "componentInstances";
			{// Add cDataMemory instance. //TODO: If you want to support other kind of data memory, this function should be further optimized.
				auto inst = gcnew OpenSmileInterop::ComponentManagerInstInstanceConfiguration();
				inst->type = "cDataMemory";
				configManagerConfig->instance->Add(gcnew Tuple<String^, OpenSmileInterop::ComponentManagerInstInstanceConfiguration^>(gcnew String("dataMemory"), inst));
			}
			for each(auto c in configs) { // auto add instances
				auto inst = gcnew OpenSmileInterop::ComponentManagerInstInstanceConfiguration();
				inst->type = c->ComponentTypeName;
				configManagerConfig->instance->Add(gcnew Tuple<String^, OpenSmileInterop::ComponentManagerInstInstanceConfiguration^>(c->InstanceName, inst));
			}
			configs->Insert(0, configManagerConfig);
		}
		
		configurations = configs;
	}

	virtual char ** findInstancesByTypeName(const char* _typename, int* N) {
		if (_typename == nullptr) {
			return nullptr;
		}
		if (N == nullptr) {
			return nullptr;
		}

		List<InstanceConfiguration^>^ configs = configurations; //unboxing
		SMILE_DBG(7, "cInteropConfigReader::findInstancesByTypeName: typename=%s", _typename);
		auto typeName = gcnew String(_typename);
		auto count = 0;
		for each(auto c in configs) {
			if (typeName->Equals(c->ComponentTypeName)) {
				count++;
			}
		}
		
		auto result = (char **)calloc(1, sizeof(char*) * count);
		auto i = 0;
		for each(auto c in configs) {
			if (typeName->Equals(c->ComponentTypeName)) {
				auto instanceName = Utility::ConvertStringToCharArray(c->InstanceName);
				result[i] = strdup(instanceName);
				i++;
				SMILE_DBG(7, "found inst : '%s'", instanceName);
				delete[] instanceName;
			}
		}

		*N = count;
		return result;
	}

	virtual ConfigInstance *getInstance(const char *_instname, const ConfigType *_type, cConfigManager *cman) {
		auto result = new ConfigInstance(_instname, _type, false);
		if (result == nullptr) {
			OUT_OF_MEMORY;
		}
		// find instance
		auto instanceName = gcnew String(_instname);
		List<InstanceConfiguration^>^ configs = configurations; //unboxing
		SMILE_DBG(7, "cInteropConfigReader::getInstance: instname=%s", _instname);
		InstanceConfiguration^ instanceConfiguration = nullptr;
		bool found = false;
		for each(instanceConfiguration in configs) {
			if (instanceName->Equals(instanceConfiguration->InstanceName)) {
				found = true;
				break;
			}
		}
		if (!found) {
			CONF_PARSER_ERR("cInteropConfigReader::getInstance: requested instance name '%s' not found in config file!", _instname);
		}
		instanceConfiguration->SetInstanceValues(result, cman);
		return result;
	}
};

#undef MODULE