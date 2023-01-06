using System;
using System.Threading.Tasks;
using OpenSense.Components.OpenPose.Common;
using OpenSense.Components.OpenPose.PInvoke.Configuration;

namespace OpenSense.Components.OpenPose.PInvoke.Session {
    public static class Session {

		#region Runtime settings
		private static bool _DebugOutput = false;

		private static void SetDebugOutput(bool enable) {
			Wrapper.OPEnableDebug(enable);
		}

		/// <summary>
		/// Print to Console
		/// </summary>
		public static bool DebugOutput {
			get => _DebugOutput;
			set {
				if (value != _DebugOutput) {
					SetDebugOutput(value);
					_DebugOutput = value;
				}
			}
		}

		private static bool _ImageOutput = false;

		private static void SetImageOutput(bool enable) {
			Wrapper.OPEnableImageOutput(enable);
		}

		/// <summary>
		/// Embed output image inside output results
		/// </summary>
		public static bool ImageOutput {
			get => _ImageOutput;
			set {
				if (value != _ImageOutput) {
					SetImageOutput(value);
					_ImageOutput = value;
				}
			}
		}

		private static bool _Output = true;

		private static void SetOutput(bool enable) {
			Wrapper.OPEnableOutput(enable);
		}

		/// <summary>
		/// Return output
		/// </summary>
		public static bool Output {
			get => _Output;
			set {
				if (value != _Output) {
					SetOutput(value);
					_Output = value;
				}
			}
		}
		#endregion

		public static Configuration.AggregateStaticConfiguration StaticConfiguration = new Configuration.AggregateStaticConfiguration();

		private static void Configure() {
			var p = StaticConfiguration.Pose;
			Wrapper.OPConfigurePose(
					p.PoseMode,
					p.NetResolution, p.OutputSize,
					p.KeypointScaleMode,
					p.GpuNumber, p.GpuNumberStart,
					p.ScalesNumber, p.ScaleGap,
					p.RenderMode,
					p.PoseModel,
					p.BlendOriginalFrame,
					p.AlphaKeypoint, p.AlphaHeatMap,
					p.DefaultPartToRender,
					p.ModelFolder,
					p.HeatMapTypes, p.HeatMapScaleMode,
					p.AddPartCandidates,
					p.RenderThreshold,
					p.NumberPeopleMax,
					p.MaximizePositives,
					p.FpsMax,
					p.ProtoTxtPath,
					p.CaffeModelPath,
					p.UpsamplingRatio
				);
			var h = StaticConfiguration.Hand;
			Wrapper.OPConfigureHand(h.Enable, h.Detector, h.InputResolution, h.ScalesNumber, h.ScaleRange, h.RenderMode, h.AlphaKeypoint, h.AlphaHeatMap, h.RenderThreshold);
			var f = StaticConfiguration.Face;
			Wrapper.OPConfigureFace(f.Enable, f.Detector, f.InputResolution, f.RenderMode, f.AlphaKeypoint, f.AlphaHeatMap, f.RenderThreshold);
			var e = StaticConfiguration.Extra;
			Wrapper.OPConfigureExtra(e.Reconstruct3d, e.MinViews3d, e.Identification, e.Tracking, e.IkThreads);
			var i = StaticConfiguration.Input;
			Wrapper.OPConfigureInput(
					i.InputType, i.ProducerString, 
					i.FrameFirst, i.FrameStep, i.FrameLast, 
					i.RealTimeProcessing, 
					i.FrameFlip, i.FrameRotate, 
					i.FramesRepeat, 
					i.CameraResolution, i.CameraParameterPath, 
					i.UndistortImage, 
					i.NumberViews
				);
			var o = StaticConfiguration.Output;
			Wrapper.OPConfigureOutput(
					o.Verbose, 
					o.WriteKeypoint, o.WriteKeypointFormat, 
					o.WriteJson, o.WriteCocoJson, o.WriteCocoJsonVariants, o.WriteCocoJsonVariant, 
					o.WriteImages, o.WriteImagesFormat, 
					o.WriteVideo, o.WriteVideoFps, o.WriteVideoWithAudio, 
					o.WriteHeatMaps, o.WriteHeatMapsFormat, 
					o.WriteVideo3D, 
					o.WriteVideoAdam, 
					o.WriteBvh, 
					o.UdpHost, o.UdpPort
				);
			var g = StaticConfiguration.Gui;
			Wrapper.OPConfigureGui(g.DisplayMode, g.GuiVerbose, g.FullScreen);
			var d = StaticConfiguration.Debug;
			Wrapper.OPConfigureDebugging(d.LogPriority, d.DisableMultiThread, d.ProfileSpeed);
		}

		public static void Initialize() {
			Wrapper.OPRegisterCallbacks();
			SetDebugOutput(_DebugOutput);
			SetImageOutput(_ImageOutput);
			SetOutput(_Output);
			Configure();
		}

		private static void WaitReady() {
			while (Wrapper.State != OPState.Ready) {
				Task.Delay(Const.BusyWaitInterval);
			}
		}

		public static void Run() {
			WaitReady();
			Wrapper.OPRun();
		}

		public static void Stop() {
			Wrapper.OPShutdown();
			WaitReady();
		}

		public static IntPtr AllocateNewFrameBuffer(int width, int height) {
			return Bind._OPAllocateNewFrameBuffer(width, height);
		}

		public static void PostNewFrame() {
			Bind._OPPostNewFrame();
		}

		public static bool GetOutput(out Datum data) {
			return Wrapper.OPGetOutput(out data);
		}
	}
}
