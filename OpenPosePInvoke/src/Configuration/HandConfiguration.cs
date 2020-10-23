using OpenPose;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace OpenPosePInvoke.Configuration {
	public class HandConfiguration : IStaticConfiguration {
		public bool Enable { get; set; } = false;
		public Detector Detector { get; set; } = Detector.Body;
		public Vector2Int? InputResolution { get; set; } = new Vector2Int(Const.DefaultNetInput1DimResolution, Const.DefaultNetInput1DimResolution);
		public int ScalesNumber { get; set; } = 1;
		public float ScaleRange { get; set; } = 0.4f;
		public RenderMode RenderMode { get; set; } = RenderMode.Auto;
		public float AlphaKeypoint { get; set; } = 0.6f;
		public float AlphaHeatMap { get; set; } = 0.7f;
		public float RenderThreshold { get; set; } = 0.2f;
	}
}
