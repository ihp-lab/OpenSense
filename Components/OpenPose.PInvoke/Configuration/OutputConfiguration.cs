namespace OpenSense.Components.OpenPose.PInvoke.Configuration {
    public class OutputConfiguration : IStaticConfiguration {
		public double Verbose { get; set; } = -1.0;
		public string WriteKeypoint { get; set; } = "";
		public DataFormat WriteKeypointFormat { get; set; } = DataFormat.Xml;
		public string WriteJson { get; set; } = "";
		public string WriteCocoJson { get; set; } = "";
		public int WriteCocoJsonVariants { get; set; } = 1;
		public int WriteCocoJsonVariant { get; set; } = 1;
		public string WriteImages { get; set; } = "";
		public string WriteImagesFormat { get; set; } = "png";
		public string WriteVideo { get; set; } = "";
		public double WriteVideoFps { get; set; } = -1.0;
		public bool WriteVideoWithAudio { get; set; } = false;
		public string WriteHeatMaps { get; set; } = "";
		public string WriteHeatMapsFormat { get; set; } = "png";
		public string WriteVideo3D { get; set; } = "";
		public string WriteVideoAdam { get; set; } = "";
		public string WriteBvh { get; set; } = "";
		public string UdpHost { get; set; } = "";
		public string UdpPort { get; set; } = "8051";
	}
}
