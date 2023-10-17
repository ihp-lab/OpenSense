#pragma once

using namespace System;

class Utility {
public:
	//caller should free returned array
	static char* ConvertStringToCharArray(String^ str) {
		if (str == nullptr) {
			return nullptr;
		}
		auto encodedBytes = Text::Encoding::UTF8->GetBytes(str);
		pin_ptr<byte> pinnedBytes = &encodedBytes[encodedBytes->GetLowerBound(0)];
		auto tokensAsUtf8 = new char[encodedBytes->Length + 1];
		memcpy(tokensAsUtf8, reinterpret_cast<char*>(pinnedBytes), encodedBytes->Length);
		tokensAsUtf8[encodedBytes->Length] = '\0';
		return tokensAsUtf8;
	}
};

