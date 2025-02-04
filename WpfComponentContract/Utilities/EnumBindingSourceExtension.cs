﻿using System;
using System.Windows.Markup;

namespace OpenSense.WPF.Components.Utilities {
    public class EnumBindingSourceExtension : MarkupExtension {

        private Type _enumType;

        public Type EnumType {
            get => _enumType;
            set {
                if (value != _enumType) {
                    if (null != value) {
                        Type enumType = Nullable.GetUnderlyingType(value) ?? value;
                        if (!enumType.IsEnum) {
                            throw new ArgumentException("Type must be for an Enum.");
                        }
                    }
                    _enumType = value;
                }
            }
        }

        public EnumBindingSourceExtension() { }

        public EnumBindingSourceExtension(Type enumType) {
            EnumType = enumType;
        }

        public override object ProvideValue(IServiceProvider serviceProvider) {
            if (_enumType is null) {
                throw new InvalidOperationException("The EnumType must be specified.");
            }

            Type actualEnumType = Nullable.GetUnderlyingType(_enumType) ?? _enumType;
            Array enumValues = Enum.GetValues(actualEnumType);

            if (actualEnumType == _enumType) {
                return enumValues;
            }

            var tempArray = Array.CreateInstance(actualEnumType, enumValues.Length + 1);
            enumValues.CopyTo(tempArray, 1);
            return tempArray;
        }
    }
}
