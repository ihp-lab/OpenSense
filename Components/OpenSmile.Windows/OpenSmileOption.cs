using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace OpenSense.Components.OpenSmile;

/// <summary>
/// Value for an option declared with \cm[...] in an openSMILE configuration, like a SMILExtract command line option.
/// </summary>
[Serializable]
public sealed class OpenSmileOption : INotifyPropertyChanged {

    private string name = string.Empty;

    /// <summary>
    /// Option name without the leading '-'
    /// </summary>
    public string Name {
        get => name;
        set => SetProperty(ref name, value);
    }

    private string value = string.Empty;

    public string Value {
        get => value;
        set => SetProperty(ref this.value, value);
    }

    #region INotifyPropertyChanged
    public event PropertyChangedEventHandler? PropertyChanged;

    private void SetProperty<T>(ref T field, T value, [CallerMemberName] string? propertyName = null) {
        if (!EqualityComparer<T>.Default.Equals(field, value)) {
            field = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
    #endregion
}
