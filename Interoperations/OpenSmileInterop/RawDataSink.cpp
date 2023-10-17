

/*  openSMILE component:

example dataSink:
reads data from data memory and outputs it to console/logfile (via smileLogger)
this component is also useful for debugging

*/

#include "SmileComponentConfiguration.h"
#include "RawDataSink.h"

#define MODULE "cRawDataSink"


SMILECOMPONENT_STATICS(cRawDataSink)

SMILECOMPONENT_REGCOMP(cRawDataSink) {
	SMILECOMPONENT_REGCOMP_INIT

	scname = COMPONENT_NAME_CRAWDATASINK;
	sdescription = COMPONENT_DESCRIPTION_CRAWDATASINK;

	// we inherit cDataSink configType and extend it:
	SMILECOMPONENT_INHERIT_CONFIGTYPE("cDataSink")

	SMILECOMPONENT_IFNOTREGAGAIN(
		ct->setField("fieldInfo", "Whether or not generate field info for each vector output", 1);
	)

	SMILECOMPONENT_MAKEINFO(cRawDataSink);
}

namespace OpenSmileInterop {
	public ref class RawDataSinkInstanceConfiguration : public DataSinkInstanceConfiguration {
		DefineField(fieldInfo, bool, false, "Whether or not generate field info for each vector output");
		
	public:

		RawDataSinkInstanceConfiguration() {
			InstanceName = "dataSink";
			componentType = cRawDataSink::typeid;
		}
		virtual void SetInstanceValuesWithFullFieldName(String^ fieldPrefix, ConfigInstance* instance, cConfigManager *cman) override {
			DataSinkInstanceConfiguration::SetInstanceValuesWithFullFieldName(fieldPrefix, instance, cman);
			SetInstance_Bool(fieldInfo);
		}

	};
}

SMILECOMPONENT_CREATE(cRawDataSink)

//-----

cRawDataSink::cRawDataSink(const char *_name) : cDataSink(_name){}

void cRawDataSink::fetchConfig() {
	cDataSink::fetchConfig();
	fieldInfo = getInt("fieldInfo") != 0;
}

int cRawDataSink::myTick(long long t) {
	cVector *vec = reader_->getFrameRel(lag);
	if (vec == nullptr) {
		return 0;
	}
	auto frame = gcnew Vector(*vec, fieldInfo);
	RawDataHandler^ handlers = rawDataHandlers;
	if (handlers != nullptr) {
		handlers(frame);
		SMILE_DBG(7, "instance %s (type: %s) invoked interop callback function", getInstName(), getTypeName());
	}
	return 1;
}