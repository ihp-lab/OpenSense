#include "pch.h"
#include "CodecOptions.h"

using namespace FFMpegInterop;

#pragma region INotifyPropertyChanging and INotifyPropertyChanged
generic <typename T>
void CodecOptions::SetProperty(T% field, T value, [NotNull] System::String^ propertyName) {
    if (!propertyName) {
        throw gcnew ArgumentNullException("propertyName");
    }
    if (!System::Collections::Generic::EqualityComparer<T>::Default->Equals(field, value)) {
        PropertyChanging(this, gcnew PropertyChangingEventArgs(propertyName));;//Checking nullptr is not necessary in C++/CLI.
        field = value;
        PropertyChanged(this, gcnew PropertyChangedEventArgs(propertyName));//Checking nullptr is not necessary in C++/CLI.
    }
}
#pragma endregion
