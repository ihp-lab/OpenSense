using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace OpenSense.DataStructure {
    [Serializable]
    public class HeadPoseAndGaze : IEquatable<HeadPoseAndGaze> {

        public readonly HeadPose HeadPose;

        public readonly Gaze Gaze;

        [JsonConstructor]
        public HeadPoseAndGaze(HeadPose headPose, Gaze gaze) {
            HeadPose = headPose;
            Gaze = gaze;
        }

        public bool Equals(HeadPoseAndGaze other) {
            return Gaze.Equals(other.Gaze) && HeadPose.Equals(other.HeadPose);
        }
    }
}
