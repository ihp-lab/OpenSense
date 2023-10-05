#pragma once

#include <core/configManager.hpp>

#include "Common.h"
#include "Configuration.h"
#include "ConfigReader.h"
#include "ComponentManager.h"
#include "RawWaveSource.h"
#include "RawDataSink.h"

using namespace System;
using namespace System::IO;
namespace OpenSmileInterop {

#define MODULE "Environment"
	public ref class Environment {
	private:

		cFileConfigReader* fileConfigReader = nullptr;
		cInteropConfigReader* interopConfigReader = nullptr;

		void inline freeMembers() {
			if (componentManager != nullptr) {
				delete componentManager;
				componentManager = nullptr;
			}
			if (configManager != nullptr) {
				delete configManager;
				configManager = nullptr;
			}
			if (fileConfigReader != nullptr) {
				//delete fileConfigReader;//not need, componentManager will free it
				fileConfigReader = nullptr;
			}
			if (interopConfigReader != nullptr) {
				//delete interopConfigReader;//not need, componentManager will free it
				interopConfigReader = nullptr;
			}
			if (commadlineParser != nullptr) {
				delete commadlineParser;
				commadlineParser = nullptr;
			}
		}

	protected:
		cCommandlineParser* commadlineParser = nullptr;//why ConfigManeger need this?
		cConfigManager* configManager = nullptr;
		cComponentManager* componentManager = nullptr;

		List<String^>^ InstanceNames(const char* componentTypeName) {
			auto result = gcnew List<String^>();
			if (componentManager != nullptr) {
				auto i = 0;
				while (true) {
					auto tempComponentTypeName = componentManager->getComponentInstanceType(i);
					if (tempComponentTypeName == nullptr) {
						break;
					}
					if (componentTypeName == nullptr || strcmp(componentTypeName, tempComponentTypeName) == 0) {
						result->Add(gcnew String(componentManager->getComponentInstance(i)->getInstName()));
					}
					i++;
				}
			}
			return result;
		}
	public:

		Environment(Configuration^ configuration) {
			try {
				//check config
				if (configuration == nullptr) {
					throw gcnew ArgumentNullException("configuration");
				}

				//log
				if (configuration->PrintLogToFile) {
					if (String::IsNullOrWhiteSpace(configuration->LogFilename)) {
						throw gcnew ArgumentException("LogFilename");
					}
					auto logFilename = Utility::ConvertStringToCharArray(configuration->LogFilename);
					LOGGER.setLogFile(logFilename, configuration->AppendLogFile, configuration->PrintLogToConsole);
					delete[] logFilename;
				} else {
					LOGGER.setLogFile((const char*)nullptr, false, configuration->PrintLogToConsole);
				}
				LOGGER.setLogLevel(configuration->OverallLogLevel);

				//init openSMILE
				commadlineParser = new cCommandlineParser(0, nullptr);
				configManager = new cConfigManager(commadlineParser);
				componentManager = new cInteropComponentManager(configManager);

				if (configuration->UseConfigurationFile) {
					//add config file reader. 
					if (configuration->ConfigurationFilename == nullptr) {
						throw gcnew ArgumentException("ConfigFilename");
					}
					if (!File::Exists(configuration->ConfigurationFilename)) {
						throw gcnew FileNotFoundException(configuration->ConfigurationFilename);
					}
					auto configFilename = Utility::ConvertStringToCharArray(configuration->ConfigurationFilename);
					fileConfigReader = new cFileConfigReader(configFilename);
					delete[] configFilename;
					configManager->addReader(fileConfigReader);
				} else {
					//add interop config reader.
					if (configuration->InstanceConfigurations == nullptr && configuration->InstanceConfigurations->Count == 0) {
						throw gcnew ArgumentException("InstanceConfigurations");
						
					} else {
						interopConfigReader = new cInteropConfigReader(configuration->InstanceConfigurations);
						configManager->addReader(interopConfigReader);
					}
				}

				//create openSMILE instances
				configManager->readConfig();
				componentManager->createInstances(0);
			} catch (cSMILException &c) {
				freeMembers();
				throw gcnew Exception(gcnew String(c.getText()));
			}
		}

		~Environment() {
			if (componentManager != nullptr) {
				componentManager->requestAbort();
			}
			freeMembers();
		}

		List<String^>^ InstanceNames() {
			return InstanceNames(nullptr);
		}

		List<String^>^ RawWaveSourceInstanceNames() {
			return InstanceNames(COMPONENT_NAME_CRAWWAVESOURCE);
		}

		List<String^>^ RawDataSinkInstanceNames() {
			return InstanceNames(COMPONENT_NAME_CRAWDATASINK);
		}

		void Hook(String^ sinkInstanceName, RawDataHandler^ callback) {
			if (String::IsNullOrEmpty(sinkInstanceName)) {
				throw gcnew ArgumentNullException("Wrong instance name");
			}
			char* name;
			try {
				name = Utility::ConvertStringToCharArray(sinkInstanceName);
				int id = componentManager->findComponentInstance(name);
				if (id < 0) {
					throw gcnew ArgumentException("Instance not found");
				}
				auto type = componentManager->getComponentInstanceType(id);
				if (_stricmp(type, COMPONENT_NAME_CRAWDATASINK) != 0) {
					throw gcnew ArgumentException("Wrong instance type");
				}
				auto instance = dynamic_cast<cRawDataSink*>(componentManager->getComponentInstance(id));
				RawDataHandler^ handlers = instance->rawDataHandlers;
				handlers += callback;
				instance->rawDataHandlers = handlers;
				SMILE_DBG(7, "Environment::Hook: successfully hooked output of instance %s (type: %s)", instance->getInstName(), instance->getTypeName());
			} finally{
				delete[] name;
			}
		}

		void Feed(String^ sourceInstanceName, array<byte>^ data) {
			if (String::IsNullOrEmpty(sourceInstanceName)) {
				throw gcnew ArgumentNullException("Wrong instance name");
			}
			char* name;
			try {				
				name = Utility::ConvertStringToCharArray(sourceInstanceName);
				int id = componentManager->findComponentInstance(name);
				if (id < 0) {
					throw gcnew ArgumentException("Instance not found");
				}
				auto type = componentManager->getComponentInstanceType(id);
				if (_stricmp(type, COMPONENT_NAME_CRAWWAVESOURCE) != 0) {
					throw gcnew ArgumentException("Wrong instance type");
				}
				auto instance = dynamic_cast<cRawWaveSource*>(componentManager->getComponentInstance(id));
				pin_ptr<byte> pinnedBuffer = &data[0];
				instance->feedData(reinterpret_cast<uint8_t*>(pinnedBuffer), data->Length);
				SMILE_DBG(7, "Environment::Feed: successfully feed %d bytes of data to input of instance %s (type: %s)", data->Length, instance->getInstName(), instance->getTypeName());
			} catch (cSMILException& ex) {
				throw gcnew Exception(gcnew String(ex.getText()));
			} finally{
				delete[] name;
			}
		}

		long long RunOneIteration() {
			return componentManager->runMultiThreaded();
		}

		
	};

}