using System.Collections.Generic;

namespace OpenSense.Components.Contract {
    /// <summary>
    /// Metadata of a component.
    /// Contains information that not need to be serialized.
    /// </summary>
    public interface IComponentMetadata {

        string Name { get; }

        string Description { get; }

        IReadOnlyList<IPortMetadata> Ports { get; }

        /// <summary>
        /// Create a default configuration object.
        /// </summary>
        ComponentConfiguration CreateConfiguration();

        object GetConnector<T>(object instance, PortConfiguration portConfiguration);
    }
}
