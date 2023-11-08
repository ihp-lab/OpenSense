#pragma once

using namespace System;
using namespace System::Collections::Generic;

namespace OpenSmileInterop {

	public ref class InstanceConfiguration abstract {
	public:
		String^ InstanceName = nullptr;

		property String^ ComponentTypeName {
			virtual String^ get() = 0;
		}

		// call by cInteropConfigReader
		virtual void SetInstanceValues(ConfigInstance* instance, cConfigManager *cman) = 0;
	};

	public ref class Configuration {
	private:

	protected:
		literal String^ DEFAULT_LOG_FILENAME = "openSMILE.log";
        literal String^ DEFAULT_CONFIG_FILENAME = "opensmile.conf";

	public:
        property List<InstanceConfiguration^>^ InstanceConfigurations;
        property bool UseConfigurationFile;
        property String^ ConfigurationFilename;
        property bool PrintLogToFile;
        property String^ LogFilename;
        property bool AppendLogFile;
        property bool PrintLogToConsole;
        property int OverallLogLevel;

        Configuration() {
            UseConfigurationFile = true;
            ConfigurationFilename = DEFAULT_CONFIG_FILENAME;
            InstanceConfigurations = gcnew List<InstanceConfiguration^>();
            LogFilename = DEFAULT_LOG_FILENAME;
        }
	};

	// component dedicate config classes

	public ref class SmileComponentInstanceConfiguration : public InstanceConfiguration {
	protected:
		Type^ componentType;

	public:
		property String^ ComponentTypeName {
			virtual String^ get() override {
				return componentType->Name;
			}
		}

		virtual void SetInstanceValues(ConfigInstance* instance, cConfigManager *cman) override sealed{
			SetInstanceValuesWithFullFieldName(nullptr, instance, cman);
		}

		virtual void SetInstanceValuesWithFullFieldName(String^ fieldPrefix, ConfigInstance* instance, cConfigManager *cman) {

		}
	};
}
