#nullable enable

namespace OpenSense.WPF.Views.Editors {
    internal sealed class OutputPortViewModel {

        public string Name { get; }

        public string? Description { get; }

        public string Type { get; }

        public OutputPortViewModel(string name, string? description, string type) {
            Name = name;
            Description = description;
            Type = type;
        }
    }
}
