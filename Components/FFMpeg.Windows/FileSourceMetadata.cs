﻿using System;
using System.Composition;

namespace OpenSense.Components.FFMpeg {
    [Export(typeof(IComponentMetadata))]
    public class FileSourceMetadata : ConventionalComponentMetadata {

        public override string Name => "FFMpeg File Source";

        public override string Description => "[Experimental] Read a file using FFMpeg. The used FFMpeg is a regular and LGPL version and is dynamically linked.";

        protected override Type ComponentType => typeof(FileSource);

        protected override string? GetPortDescription(string portName) {
            switch (portName) {
                case nameof(FileSource.Out):
                    return "The output frames.";
                default:
                    return null;
            }
        }

        public override ComponentConfiguration CreateConfiguration() => new FileSourceConfiguration();
    }
}

