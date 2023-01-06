namespace OpenSense.Components.OpenPose.PInvoke.Configuration {
    public class GuiConfiguration : IStaticConfiguration {
		public DisplayMode DisplayMode { get; set; } = DisplayMode.NoDisplay;
		public bool GuiVerbose { get; set; } = false;
		public bool FullScreen { get; set; } = false;
	}
}
