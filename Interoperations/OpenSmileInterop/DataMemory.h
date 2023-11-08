#pragma once
#include <core/dataMemory.hpp>

namespace OpenSmileInterop {

	using namespace System;

	public ref class Field {
	public:
		property String^ Name;
		property int Length;
		Field(const FieldMetaInfo& fieldMetaInfo) {
			Name = gcnew String(fieldMetaInfo.name);
			Length = fieldMetaInfo.N;
		}
	};

	public ref class Vector {

	protected:

	public:
		///real start time in seconds
		property double Time;
		///index of this frame in data memory level
		property long Index;
		///real length in seconds
		property double LengthSec;
		property Type^ DataType;
		property array<Field^>^ FieldInfo;
		property array<array<byte>^>^ Data;

		Vector(const cVector& vector, const bool generateFieldInfo) {
			//properties
			Time = vector.tmeta->time;
			if (vector.tmetaArr) {
				throw gcnew Exception("cVector.tmetaArr not supported");
			}
			Index = vector.tmeta->vIdx;
			LengthSec = vector.tmeta->lengthSec;

			//data type
			int elemSize;
			char* source;
			static_assert(sizeof(FLOAT_DMEM) == sizeof(INT_DMEM), "FLOAT_DMEM and INT_DMEM should be the same size in bytes");
			switch (vector.type) {
			case DMEM_FLOAT:
				DataType = float::typeid;
				elemSize = sizeof(FLOAT_DMEM);
				source = reinterpret_cast<char*>(vector.dataF);
				break;
			case DMEM_INT:
				DataType = int::typeid;
				elemSize = sizeof(INT_DMEM);
				source = reinterpret_cast<char*>(vector.dataI);
			default:
				throw gcnew Exception("Unknown openSMILE data type");
			}

			//data
			Data = gcnew array<array<byte>^>(vector.N);
			for (auto i = 0; i < vector.N; i++) {
				Data[i] = gcnew array<byte>(elemSize);
				pin_ptr<byte> pinned = &Data[i][0];
				memcpy(reinterpret_cast<char*>(pinned), source + i * elemSize, elemSize);
			}

			//fields
			if (generateFieldInfo) {
				FieldInfo = gcnew array<Field^>(vector.fmeta->N);
				for (auto i = 0; i < vector.fmeta->N; i++) {
					auto& field = vector.fmeta->field[i];
					FieldInfo[i] = gcnew Field(field);
				}
			} else {
				FieldInfo = nullptr;
			}
		}
	};

}

