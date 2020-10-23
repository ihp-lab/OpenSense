using System;
using System.Collections.Generic;
using System.Text;

namespace OpenPosePInvoke.Configuration {
	public class ExtraConfiguration : IStaticConfiguration {
		public bool Reconstruct3d { get; set; } = false;
		public int MinViews3d { get; set; } = -1;
		public bool Identification { get; set; } = false;
		public int Tracking { get; set; } = -1;
		public int IkThreads { get; set; } = 0;
	}
}
