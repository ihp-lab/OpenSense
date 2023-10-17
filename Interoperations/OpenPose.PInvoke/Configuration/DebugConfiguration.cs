namespace OpenSense.Components.OpenPose.PInvoke.Configuration {
    public class DebugConfiguration : IStaticConfiguration {
		public Priority LogPriority { get; set; } = Priority.High;
		public bool DisableMultiThread { get; set; } = false;
		public ulong ProfileSpeed { get; set; } = 1000;
	}
}
