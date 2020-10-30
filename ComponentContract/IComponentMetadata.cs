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

        object GetConnector<T>(object instance, PortConfiguration portConfiguration);
    }
}
