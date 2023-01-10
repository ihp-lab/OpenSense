// OpenPose Unity Plugin v1.0.0alpha-1.5.0
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

namespace OpenSense.Components.OpenPose.PInvoke {
    /*
	 * Function set to parse output
	 */
    public static class OutputParser {

		public static void ParseOutput(ref Datum datum, IntPtr[] ptrArray, int[] sizeArray, OutputType type){
			switch (type){
				case OutputType.None: break;
				case OutputType.DatumsInfo: ParseDatumsInfo(ref datum, ptrArray, sizeArray); break;
				case OutputType.Name: ParseName(ref datum, ptrArray, sizeArray); break;
				case OutputType.PoseKeypoints: ParsePoseKeypoints(ref datum, ptrArray, sizeArray); break;
				case OutputType.PoseIds: ParsePoseIds(ref datum, ptrArray, sizeArray); break;
				case OutputType.PoseScores: ParsePoseScores(ref datum, ptrArray, sizeArray); break;
				case OutputType.FaceRectangles: ParseFaceRectangles(ref datum, ptrArray, sizeArray); break;
				case OutputType.FaceKeypoints: ParseFaceKeypoints(ref datum, ptrArray, sizeArray); break;
				case OutputType.HandRectangles: ParseHandRectangles(ref datum, ptrArray, sizeArray); break;
				case OutputType.HandKeypoints: ParseHandKeypoints(ref datum, ptrArray, sizeArray); break;
				case OutputType.Image: ParseImage(ref datum, ptrArray, sizeArray); break;
				default: Debug.Log("Output type not supported yet: " + type); break;
			}
		}
        // Parse id, subId, subIdMax, frameNumber
		public static void ParseDatumsInfo(ref Datum datum, IntPtr[] ptrArray, int[] sizeArray){
			Debug.AssertFormat(ptrArray.Length == 4, "DatumsInfo array length invalid: " + ptrArray.Length);
			Debug.AssertFormat(sizeArray.Length == 1, "DatumsInfo size length invalid: " + sizeArray.Length);
			var temp = new long[4]; // use long to marshal
			Marshal.Copy(ptrArray[0], temp, 0, 1);
			Marshal.Copy(ptrArray[1], temp, 1, 1);
			Marshal.Copy(ptrArray[2], temp, 2, 1);
			Marshal.Copy(ptrArray[3], temp, 3, 1);
			datum.id = (ulong)temp[0];
			datum.subId = (ulong)temp[1];
			datum.subIdMax = (ulong)temp[2];
			datum.frameNumber = (ulong)temp[3];
		}
        // Parse name
        public static void ParseName(ref Datum datum, IntPtr[] ptrArray, int[] sizeArray){
			Debug.AssertFormat(ptrArray.Length == 1, "Name array length invalid: " + ptrArray.Length);
			Debug.AssertFormat(sizeArray.Length == 1, "Name size length invalid: " + sizeArray.Length);
			datum.name = Marshal.PtrToStringAnsi(ptrArray[0]);
		}
        // Parse poseKeypoints
		public static void ParsePoseKeypoints(ref Datum datum, IntPtr[] ptrArray, int[] sizeArray){
			Debug.AssertFormat(ptrArray.Length == 1, "PoseKeypoints array length invalid: " + ptrArray.Length);
			Debug.AssertFormat(sizeArray.Length == 3, "PoseKeypoints size length invalid: " + sizeArray.Length);
            int volume = 1;
            foreach(var i in sizeArray){ volume *= i; }
			if (volume == 0) return;
			
			var valArray = new float[volume];
			Marshal.Copy(ptrArray[0], valArray, 0, volume);
			datum.poseKeypoints = new MultiArray<float>(valArray, sizeArray);
		}
        // Parse poseIds
		public static void ParsePoseIds(ref Datum datum, IntPtr[] ptrArray, int[] sizeArray){
			Debug.AssertFormat(ptrArray.Length == 1, "PoseIds array length invalid: " + ptrArray.Length);
			Debug.AssertFormat(sizeArray.Length == 1, "PoseIds size length invalid: " + sizeArray.Length);
            int volume = 1;
            foreach(var i in sizeArray){ volume *= i; }
			if (volume == 0) return;
			
			var valArray = new long[volume];
			Marshal.Copy(ptrArray[0], valArray, 0, volume);
			datum.poseIds = new MultiArray<long>(valArray, sizeArray);
		}
        // Parse poseScores
        public static void ParsePoseScores(ref Datum datum, IntPtr[] ptrArray, int[] sizeArray){
			Debug.AssertFormat(ptrArray.Length == 1, "PoseScores array length invalid: " + ptrArray.Length);
			Debug.AssertFormat(sizeArray.Length == 1, "PoseScores size length invalid: " + sizeArray.Length);
            int volume = 1;
            foreach(var i in sizeArray){ volume *= i; }
			if (volume == 0) return;
			
			var valArray = new float[volume];
			Marshal.Copy(ptrArray[0], valArray, 0, volume);
			datum.poseScores = new MultiArray<float>(valArray, sizeArray);
		}
        // Parse poseHeatMaps
        public static void ParsePoseHeatMaps(ref Datum datum, IntPtr[] ptrArray, int[] sizeArray){
			Debug.AssertFormat(ptrArray.Length == 1, "PoseHeatMaps array length invalid: " + ptrArray.Length);
			Debug.AssertFormat(sizeArray.Length == 3, "PoseHeatMaps size length invalid: " + sizeArray.Length);
            int volume = 1;
            foreach(var i in sizeArray){ volume *= i; }
			if (volume == 0) return;
			
			var valArray = new float[volume];
			Marshal.Copy(ptrArray[0], valArray, 0, volume);
			datum.poseHeatMaps = new MultiArray<float>(valArray, sizeArray);
		}
        // Parse handKeypoints
        public static void ParseHandKeypoints(ref Datum datum, IntPtr[] ptrArray, int[] sizeArray){
			Debug.AssertFormat(ptrArray.Length == 2, "HandKeypoints array length invalid: " + ptrArray.Length);
			Debug.AssertFormat(sizeArray.Length == 3, "HandKeypoints size length invalid: " + sizeArray.Length);
			int volume = 1;
            foreach(var i in sizeArray){ volume *= i; }
			if (volume == 0) return;
			
			// Left
			var valArrayL = new float[volume];
			Marshal.Copy(ptrArray[0], valArrayL, 0, volume);
			var handKeypointsL = new MultiArray<float>(valArrayL, sizeArray);
			// Right
			var valArrayR = new float[volume];
			Marshal.Copy(ptrArray[1], valArrayR, 0, volume);
			var handKeypointsR = new MultiArray<float>(valArrayR, sizeArray);

			datum.handKeypoints = new Pair<MultiArray<float>>(handKeypointsL, handKeypointsR);
		}
        // Parse handRectangles
        public static void ParseHandRectangles(ref Datum datum, IntPtr[] ptrArray, int[] sizeArray){
			Debug.AssertFormat(sizeArray.Length == 2, "HandKeypoints size length invalid: " + sizeArray.Length);
			Debug.AssertFormat(sizeArray[0] == 2 && sizeArray[1] == 4, "HandKeypoints sizes invalid");
			datum.handRectangles = new List<Pair<Rect>>();
			foreach(var ptr in ptrArray){
				var tempData = new float[8];
				Marshal.Copy(ptr, tempData, 0, 8);
				Rect left = new Rect(tempData[0], tempData[1], tempData[2], tempData[3]);
				Rect right = new Rect(tempData[4], tempData[5], tempData[6], tempData[7]);
				datum.handRectangles.Add(new Pair<Rect>(left, right));
			}
		}
        // Parse handHeatMaps
        public static void ParseHandHeatMaps(ref Datum datum, IntPtr[] ptrArray, int[] sizeArray){
			Debug.AssertFormat(ptrArray.Length == 2, "HandHeatMaps array length invalid: " + ptrArray.Length);
			Debug.AssertFormat(sizeArray.Length == 4, "HandHeatMaps size length invalid: " + sizeArray.Length);
            int volume = 1;
            foreach(var i in sizeArray){ volume *= i; }
			if (volume == 0) return;
			
			// Left
			var valArrayL = new float[volume];
			Marshal.Copy(ptrArray[0], valArrayL, 0, volume);
			var handHeatMapsL = new MultiArray<float>(valArrayL, sizeArray);
			// Right
			var valArrayR = new float[volume];
			Marshal.Copy(ptrArray[1], valArrayR, 0, volume);
			var handHeatMapsR = new MultiArray<float>(valArrayR, sizeArray);

			datum.handHeatMaps = new Pair<MultiArray<float>>(handHeatMapsL, handHeatMapsR);
		}
        // Parse faceKeypoints
        public static void ParseFaceKeypoints(ref Datum datum, IntPtr[] ptrArray, int[] sizeArray){
			Debug.AssertFormat(ptrArray.Length == 1, "FaceKeypoints array length invalid: " + ptrArray.Length);
			Debug.AssertFormat(sizeArray.Length == 3, "FaceKeypoints size length invalid: " + ptrArray.Length);
            int volume = 1;
            foreach(var i in sizeArray){ volume *= i; }
			if (volume == 0) return;
			
			var valArray = new float[volume];
			Marshal.Copy(ptrArray[0], valArray, 0, volume);
			datum.faceKeypoints = new MultiArray<float>(valArray, sizeArray);
		}
        // Parse faceRectangles
        public static void ParseFaceRectangles(ref Datum datum, IntPtr[] ptrArray, int[] sizeArray){
			Debug.AssertFormat(ptrArray.Length == 1, "FaceRect array length is invalid: " + ptrArray.Length);
			Debug.AssertFormat(sizeArray.Length == 2, "FaceRect size length is invalid: " + sizeArray.Length);
            int volume = 1;
            foreach(var i in sizeArray){ volume *= i; }
			if (volume == 0) return;
			
			var valArray = new float[volume];
			Marshal.Copy(ptrArray[0], valArray, 0, volume);

            // Using UnityEngine.Rect
			var list = new List<Rect>();
			for (int i = 0; i < sizeArray[0]; i++){
				list.Add(new Rect(valArray[i * 4 + 0], valArray[i * 4 + 1], valArray[i * 4 + 2], valArray[i * 4 + 3]));
			}
			
			datum.faceRectangles = list;
		}
        // Parse faceHeatMaps
        public static void ParseFaceHeatMaps(ref Datum datum, IntPtr[] ptrArray, int[] sizeArray){
			Debug.AssertFormat(ptrArray.Length == 1, "FaceHeatMaps array length invalid: " + ptrArray.Length);
			Debug.AssertFormat(sizeArray.Length == 4, "FaceHeatMaps size length invalid: " + sizeArray.Length);
            int volume = 1;
            foreach(var i in sizeArray){ volume *= i; }
			if (volume == 0) return;
			
			var valArray = new float[volume];
			Marshal.Copy(ptrArray[0], valArray, 0, volume);
			datum.faceHeatMaps = new MultiArray<float>(valArray, sizeArray);
		}
        // Parse cvInputData
        public static void ParseImage(ref Datum datum, IntPtr[] ptrArray, int[] sizeArray){
			Debug.AssertFormat(ptrArray.Length == 1, "Image array length invalid: " + ptrArray.Length);
			Debug.AssertFormat(sizeArray.Length == 3, "Image size length invalid: " + sizeArray.Length);
            int volume = 1;
            foreach(var i in sizeArray){ volume *= i; }
			if (volume == 0) return;
			
			var valArray = new byte[volume];
			Marshal.Copy(ptrArray[0], valArray, 0, volume);
			datum.cvInputData = new MultiArray<byte>(valArray, sizeArray);
		}
	}
}
