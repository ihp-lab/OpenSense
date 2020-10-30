using System.Collections.Generic;
using Microsoft.Psi;

namespace OpenSense.Component.Contract {
    /// <summary>
    /// Metadata of a component.
    /// </summary>
    public interface IComponentMetadata {

        string Name { get; }

        string Description { get; }

        IReadOnlyList<IPortMetadata> Ports { get; }

        /// <summary>
        /// Create a default configuration object.
        /// </summary>
        /// <returns></returns>
        ComponentConfiguration CreateConfiguration();

        /// <summary>
        /// Method to get an output connector (such as IProducer<T>) given the configuration of the port.
        /// </summary>
        /// <typeparam name="T">Data type that a consumer will accept.</typeparam>
        /// <param name="instance"></param>
        /// <param name="portConfiguration"></param>
        /// <returns></returns>
        object GetOutputConnector<T>(object instance, PortConfiguration portConfiguration);
    }
}
