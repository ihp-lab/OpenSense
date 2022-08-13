using System;
using OpenSense.Component.Contract;

namespace OpenSense.Component.PythonSupports {
    public static class PortHelper {

        public static IPortMetadata In(string name, Type type, string description = null) {
            var result = new PythonPortMetadata(name, PortDirection.Input, type, description);
            return result;
        }

        public static IPortMetadata Out(string name, Type type, string description = null) {
            var result = new PythonPortMetadata(name, PortDirection.Output, type, description);
            return result;
        }
    }
}
