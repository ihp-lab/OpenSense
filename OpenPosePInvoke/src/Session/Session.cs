using OpenPose;
using OpenPosePInvoke.Configuration;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace OpenPosePInvoke.Session {
	public static class Session {

		#region Runtime settings
		private static bool _DebugOutput = false;

		private static void SetDebugOutput(bool enable) {
			OPWrapper.OPEnableDebug(enable);
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
			OPWrapper.OPEnableImageOutput(enable);
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
			OPWrapper.OPEnableOutput(enable);
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

		public static StaticConfiguration StaticConfiguration = new StaticConfiguration();

		private static void Configure() {
			var p = StaticConfiguration.Pose;
			OPWrapper.OPConfigurePose(
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
			OPWrapper.OPConfigureHand(h.Enable, h.Detector, h.InputResolution, h.ScalesNumber, h.ScaleRange, h.RenderMode, h.AlphaKeypoint, h.AlphaHeatMap, h.RenderThreshold);
			var f = StaticConfiguration.Face;
			OPWrapper.OPConfigureFace(f.Enable, f.Detector, f.InputResolution, f.RenderMode, f.AlphaKeypoint, f.AlphaHeatMap, f.RenderThreshold);
			var e = StaticConfiguration.Extra;
			OPWrapper.OPConfigureExtra(e.Reconstruct3d, e.MinViews3d, e.Identification, e.Tracking, e.IkThreads);
			var i = StaticConfiguration.Input;
			OPWrapper.OPConfigureInput(
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
			OPWrapper.OPConfigureOutput(
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
			OPWrapper.OPConfigureGui(g.DisplayMode, g.GuiVerbose, g.FullScreen);
			var d = StaticConfiguration.Debug;
			OPWrapper.OPConfigureDebugging(d.LogPriority, d.DisableMultiThread, d.ProfileSpeed);
		}

		public static void Initialize() {
			OPWrapper.OPRegisterCallbacks();
			SetDebugOutput(_DebugOutput);
			SetImageOutput(_ImageOutput);
			SetOutput(_Output);
			Configure();
		}

		private static void WaitReady() {
			while (OPWrapper.State != OPState.Ready) {
				Task.Delay(Const.BusyWaitInterval);
			}
		}

		public static void Run() {
			WaitReady();
			OPWrapper.OPRun();
		}

		public static void Stop() {
			OPWrapper.OPShutdown();
			WaitReady();
		}

		public static IntPtr AllocateNewFrameBuffer(int width, int height) {
			return OPBind._OPAllocateNewFrameBuffer(width, height);
		}

		public static void PostNewFrame() {
			OPBind._OPPostNewFrame();
		}

		public static bool GetOutput(out OpenPoseDatum data) {
			return OPWrapper.OPGetOutput(out data);
		}
	}
}
