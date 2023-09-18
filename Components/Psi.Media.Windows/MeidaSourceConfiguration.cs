using System;
using Microsoft.Psi.Media;

namespace OpenSense.Components.Psi.Media {
    [Serializable]
    public class MediaSourceConfiguration : ConventionalComponentConfiguration {

        private string filename;

        public string Filename {
            get => filename;
            set => SetProperty(ref filename, value);
        }

        private bool dropOutOfOrderPackets = false;

        public bool DropOutOfOrderPackets {
            get => dropOutOfOrderPackets;
            set => SetProperty(ref dropOutOfOrderPackets, value);
        }

        public override IComponentMetadata GetMetadata() => new MediaSourceMetadata();

        protected override object Instantiate(Microsoft.Psi.Pipeline pipeline, IServiceProvider serviceProvider) => new MediaSource(pipeline, Filename, DropOutOfOrderPackets);
    }
}
