using OpenPose;
using System;
using System.Collections.Generic;
using System.Text;

namespace OpenPosePInvoke.Configuration {
	public class GuiConfiguration : IStaticConfiguration {
		public DisplayMode DisplayMode { get; set; } = DisplayMode.NoDisplay;
		public bool GuiVerbose { get; set; } = false;
		public bool FullScreen { get; set; } = false;
	}
}
