
/*  openSMILE component:

raw wave frame dataSource
writes data to data memory...

*/

#include <core/exceptions.hpp>
#include <smileutil/smileUtil.h>

#include "SmileComponentConfiguration.h"
#include "RawWaveSource.h"

#define MODULE "cRawWaveSource"

SMILECOMPONENT_STATICS(cRawWaveSource)

SMILECOMPONENT_REGCOMP(cRawWaveSource) {
	SMILECOMPONENT_REGCOMP_INIT
	scname = COMPONENT_NAME_CRAWWAVESOURCE;
	sdescription = COMPONENT_DESCRIPTION_CRAWWAVESOURCE;

	// we inherit cDataSource configType and extend it:
	SMILECOMPONENT_INHERIT_CONFIGTYPE("cDataSource")

		SMILECOMPONENT_IFNOTREGAGAIN(
		ct->setField("monoMixdown", "Mix down all channels to 1 mono channel (1=on, 0=off)", 1);
		ct->setField("sampleRate", "Set the sampling rate that is assigned to the input data", 16000, 0, 0);
		ct->setField("sampleSize", "Set the samplesize (in bytes, each channel)", 2, 0, 0); // can be moved to runtime
		ct->setField("channels", "Set the number of channels", 2, 0, 0);
		ct->setField("outFieldName", "Set the name of the output field, containing the pcm data", "pcm");

		ct->disableField("period");// Inherited from cDataSource. We will compute this value by ourself.
		ct->setField("blocksize_sec", NULL, 1.0);// overwrite cDataSource's default
	)

	SMILECOMPONENT_MAKEINFO(cRawWaveSource);
}

namespace OpenSmileInterop {
	public ref class RawWaveSourceInstanceConfiguration : public DataSourceInstanceConfiguration {
		DefineField(monoMixdown, bool, true, "");
		DefineField(sampleRate, int, 16000, "");
		DefineField(sampleSize, int, 2, "# of bytes per sample");
		DefineField(channels, int, 2, "");
		DefineField(outFieldName, String^, "pcm", "");

	public:

		RawWaveSourceInstanceConfiguration() {
			InstanceName = "waveSource";
			componentType = cRawWaveSource::typeid;
			// disable
			DisableField(period);
			// overwrite
			ChangeFieldDefault(blocksize_sec, 1.0);
		}

		virtual void SetInstanceValuesWithFullFieldName(String^ fieldPrefix, ConfigInstance* instance, cConfigManager *cman) override {
			DataSourceInstanceConfiguration::SetInstanceValuesWithFullFieldName(fieldPrefix, instance, cman);
			SetInstance_Bool(monoMixdown);
			SetInstance_Int(sampleRate);
			SetInstance_Int(sampleSize);
			SetInstance_Int(channels);
			SetInstance_String(outFieldName);
		}
	};
}

SMILECOMPONENT_CREATE(cRawWaveSource)

//-----

cRawWaveSource::cRawWaveSource(const char *_name) : cDataSource(_name) { }

void cRawWaveSource::fetchConfig() {
	cDataSource::fetchConfig();

	outFieldName = getStr("outFieldName");
	if (outFieldName == NULL) COMP_ERR("fetchConfig: getStr(outFieldName) returned NULL! missing option in config file?");

	monoMixdown = getInt("monoMixdown") != 0;

	sampleRate = getInt("sampleRate");
	channels = getInt("channels");
	sampleSize = getInt("sampleSize");
}

int cRawWaveSource::configureWriter(sDmLevelConfig &c) {
	// configure writer
	c.T = 1.0 / (double)(sampleRate); // period of each frame/sample
	return 1;
}

// NOTE: nEl is always 0 for dataSources....
int cRawWaveSource::setupNewNames(long nEl) {
	// configure dateMemory level, names, etc.
	writer_->addField(outFieldName, monoMixdown ? 1 : channels);
	return 1;
}

int cRawWaveSource::myTick(long long t) {// reference cWaveSource::myTick
	if (isEOI()) {
		SMILE_IERR(1, "Processing aborted!");
		return 0;
	}
	if (mat_ == nullptr) {
		allocMat(monoMixdown ? 1: channels, blocksizeW_);
	}
	if (writer_->checkWrite(blocksizeW_)) {
		if (readData()) {
			if (!writer_->setNextMatrix(mat_)) { // save data in dataMemory buffers
				SMILE_IERR(1, "can't write, level full... (strange, level space was checked using checkWrite(bs=%i))", blocksizeW_);
			} else {
				return 1;
			}
		}
	}
	return 0;
}

// reads data into matrix m, size is determined by m, also performs format conversion to float samples and matrix format
bool cRawWaveSource::readData() {
	//std::lock_guard<std::mutex> lock(bufferLock);
	sWaveParameters tempPcmParam;
	tempPcmParam.sampleRate = sampleRate;
	tempPcmParam.nChan = channels;
	tempPcmParam.nBPS = sampleSize;
	tempPcmParam.nBits = sampleSize * 8;
	tempPcmParam.blockSize = channels * sampleSize;
	long samplesRemain = buffer.size() / tempPcmParam.blockSize;
	cMatrix *m = nullptr;
	m = mat_;
	long samplesToRead = m->nT;
	if (samplesRemain < blocksizeW_) {
		samplesToRead = samplesRemain;
	}
	
	// check for float data type
	// if they match, convert with smilePcm_readSamples();
	long nRead = 0;
#if FLOAT_DMEM_NUM == FLOAT_DMEM_FLOAT
	nRead = smilePcm_convertSamples(buffer.data(), &tempPcmParam, mat_->dataF, tempPcmParam.nChan, samplesToRead, monoMixdown);
#else
	// TODO: allocate only once, put variable in class object
	float * a = (float *)malloc(sizeof(float) * m->nT);
	nRead = smilePcm_convertSamples(buffer.data(), &tempPcmParam, a, tempPcmParam.nChan, samplesToRead, monoMixdown);
	// convert to matrix
	for (long i = 0; i < nRead && i < m->nT; i++) {
		m->dataF[i] = (FLOAT_DMEM)a[i];
	}
	free(a);
#endif
	if (nRead != blocksizeW_ || nRead < 0) {
		SMILE_DBG(5, "nRead (%i) < size to read (%i) ==> buffer empty!", nRead, blocksizeW_);
		m->nT = nRead >= 0 ? nRead : 0;
	}
	if (nRead > 0) {
		buffer.erase(buffer.begin(), buffer.begin() + nRead * tempPcmParam.blockSize);
	}
	return nRead > 0;
}

void cRawWaveSource::feedData(const uint8_t * data, const int dataLength) {
	//std::lock_guard<std::mutex> lock(bufferLock);
	buffer.insert(buffer.end(), *data, *data + dataLength);
	SMILE_DBG(7, "instance %s (type: %s) buffered %d bytes of data, now remain %d bytes", getInstName(), getTypeName(), dataLength, buffer.size());
}