using UnityEngine;

namespace OpenSense.Components.OpenPose.PInvoke.Configuration {
    public class InputConfiguration : IStaticConfiguration {
		public ProducerType InputType { get; set; } = ProducerType.Webcam;
		public string ProducerString { get; set; } = "-1";
		public ulong FrameFirst { get; set; } = 0;
		public ulong FrameStep { get; set; } = 1;
		public ulong FrameLast { get; set; } = ulong.MaxValue;
		public bool RealTimeProcessing { get; set; } = false;
		public bool FrameFlip { get; set; } = false;
		public int FrameRotate { get; set; } = 0;
		public bool FramesRepeat { get; set; } = false;
		public Vector2Int? CameraResolution { get; set; } = null;
		public string CameraParameterPath { get; set; } = null;
		public bool UndistortImage { get; set; } = false;
		public int NumberViews { get; set; } = -1;
	}
}
