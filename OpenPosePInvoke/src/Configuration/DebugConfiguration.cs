using OpenPose;
using System;
using System.Collections.Generic;
using System.Text;

namespace OpenPosePInvoke.Configuration {
	public class DebugConfiguration : IStaticConfiguration {
		public Priority LogPriority { get; set; } = Priority.High;
		public bool DisableMultiThread { get; set; } = false;
		public ulong ProfileSpeed { get; set; } = 1000;
	}
}
