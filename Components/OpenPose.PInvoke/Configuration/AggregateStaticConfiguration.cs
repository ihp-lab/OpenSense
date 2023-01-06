namespace OpenSense.Components.OpenPose.PInvoke.Configuration {
    public class AggregateStaticConfiguration : IStaticConfiguration {
		public PoseConfiguration Pose { get; set; } = new PoseConfiguration();
		public HandConfiguration Hand { get; set; } = new HandConfiguration();
		public FaceConfiguration Face { get; set; } = new FaceConfiguration();
		public ExtraConfiguration Extra { get; set; } = new ExtraConfiguration();
		public InputConfiguration Input { get; set; } = new InputConfiguration();
		public OutputConfiguration Output { get; set; } = new OutputConfiguration();
		public GuiConfiguration Gui { get; set; } = new GuiConfiguration();
		public DebugConfiguration Debug { get; set; } = new DebugConfiguration();
	}
}
