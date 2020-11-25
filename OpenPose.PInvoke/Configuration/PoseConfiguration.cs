using UnityEngine;

namespace OpenSense.Component.OpenPose.PInvoke.Configuration {
    public class PoseConfiguration : IStaticConfiguration {
		public PoseMode PoseMode { get; set; } = PoseMode.Enabled;
		public Vector2Int? NetResolution { get; set; } = new Vector2Int(-1, Const.DefaultNetInput1DimResolution);
		public Vector2Int? OutputSize { get; set; } = null;
		public ScaleMode KeypointScaleMode { get; set; } = ScaleMode.InputResolution;
		public int GpuNumber { get; set; } = -1;
		public int GpuNumberStart { get; set; } = 0;
		public int ScalesNumber { get; set; } = 1;
		public float ScaleGap { get; set; } = 0.25f;
		public RenderMode RenderMode { get; set; } = RenderMode.Auto;
		public PoseModel PoseModel { get; set; } = PoseModel.BODY_25;
		public bool BlendOriginalFrame { get; set; } = true;
		public float AlphaKeypoint { get; set; } = 0.6f;
		public float AlphaHeatMap { get; set; } = 0.7f;
		public int DefaultPartToRender { get; set; } = 0;
		public string ModelFolder { get; set; } = null;
		public HeatMapType HeatMapTypes { get; set; } = HeatMapType.None;
		public ScaleMode HeatMapScaleMode { get; set; } = ScaleMode.ZeroToOne;
		public bool AddPartCandidates { get; set; } = false;
		public float RenderThreshold { get; set; } = 0.05f;
		public int NumberPeopleMax { get; set; } = -1;
		public bool MaximizePositives { get; set; } = false;
		public double FpsMax { get; set; } = -1.0;
		public string ProtoTxtPath { get; set; } = "";
		public string CaffeModelPath { get; set; } = "";
		public float UpsamplingRatio { get; set; } = 0f;
	}
}
