using System.Collections.Immutable;
using System.Diagnostics;
using System.Numerics;
using Microsoft.Psi;
using OpenSense.Components.OpenFace;

namespace HeadGestureDetector.App.Consoles {
    internal sealed class PoseSource : IProducer<Pose> {

        private static readonly TimeSpan Resolution = TimeSpan.FromMilliseconds(1);

        private readonly double _frameRate;

        public Emitter<Pose> Out { get; }

        private DateTime pipelineStartTime;

        private DateTime lastTimestamp;

        public PoseSource(Pipeline pipeline, double frameRate) {
            _frameRate = frameRate;
            Out = pipeline.CreateEmitter<Pose>(this, nameof(Out));
            pipeline.PipelineRun += OnPipelineRun;
        }

        private void OnPipelineRun(object? sender, PipelineRunEventArgs args) {
            pipelineStartTime = args.StartOriginatingTime;
        }

        public DateTime Post(
            long frameIndex,
            Vector3 position,
            Vector3 angle
        ) {
            var timestamp = pipelineStartTime + TimeSpan.FromSeconds(frameIndex / _frameRate);
            Trace.Assert(timestamp > lastTimestamp + Resolution);
            var pose = new Pose(
                position, 
                angle, 
                ImmutableArray<Vector2>.Empty, 
                ImmutableArray<Vector2>.Empty, 
                ImmutableArray<Vector3>.Empty, 
                ImmutableArray<(Vector2, Vector2)>.Empty
            );
            Out.Post(pose, timestamp);
            lastTimestamp = timestamp;
            return timestamp;
        }

        public void Complete() {
            Out.Close(lastTimestamp);
        }
    }
}
