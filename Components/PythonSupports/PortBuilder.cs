using System;
using OpenSense.Components.Contract;

namespace OpenSense.Components.PythonSupports {

    public sealed class PortBuilder {

        #region Static Methods
        public static PortBuilder Create() {
            var result = new PortBuilder();
            return result;
        }
        #endregion

        #region Properties
        internal string Name { get; set; } = "Out";

        internal PortDirection Direction { get; set; } = PortDirection.Output;

        internal Type Type { get; set; } = typeof(double);

        internal string Description { get; set; } = string.Empty;
        #endregion

        #region Constructors
        private PortBuilder() {

        }

        internal PortBuilder(PortBuilder other) {
            Name = other.Name;
            Direction = other.Direction;
            Type = other.Type;
            Description = other.Description;
        }
        #endregion

        #region Methods
        public IPortMetadata Build() {
            var result = new PythonPortMetadata(Name, Direction, Type, Description);
            return result;
        }

        public PortBuilder AsInput() {
            var result = new PortBuilder(this) {
                Direction = PortDirection.Input,
            };
            return result;
        }

        public PortBuilder AsOutput() {
            var result = new PortBuilder(this) {
                Direction = PortDirection.Output,
            };
            return result;
        }

        public PortBuilder WithName(string name) {
            var result = new PortBuilder(this) {
                Name = name,
            };
            return result;
        }

        public PortBuilder WithDescription(string description) {
            var result = new PortBuilder(this) {
                Description = description,
            };
            return result;
        }

        public PortBuilder WithType(Type type) {
            var result = new PortBuilder(this) {
                Type = type,
            };
            return result;
        }

        public PortBuilder MakeArray() {
            var arrayType = Type.MakeArrayType();
            var result = new PortBuilder(this) {
                Type = arrayType,
            };
            return result;
        }
        #endregion
    }
}
