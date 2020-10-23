using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

namespace OpenPose {

	public static class OpenPoseInputParser {
		/*
		public static byte[][] buffers ParseInput(OPDatum datum, OutputType type) {//TODO: prefer return Memory<T> in .net standard 2.0
			switch (type) {
				case OutputType.Image:
					return ParseImage(datum);
				default:
					var message = "Output type not supported yet: " + type;
					Debug.Log(message);
					throw new NotSupportedException(message);
			}
		}
		
		// Parse cvInputData
		public static byte[][] buffers ParseImage(OPDatum datum) {
			Debug.AssertFormat(ptrArray.Length == 1, "Image array length invalid: " + ptrArray.Length);
			Debug.AssertFormat(sizeArray.Length == 3, "Image size length invalid: " + sizeArray.Length);
			int volume = 1;
			foreach (var i in sizeArray) { volume *= i; }
			if (volume == 0) return;

			var valArray = new byte[volume];
			Marshal.Copy(ptrArray[0], valArray, 0, volume);
			datum.cvInputData = new MultiArray<byte>(valArray, sizeArray);

			
			var data = datum.cvInputData;
			var dim = data?.GetNumberDimensions() ?? 0;
			var result = new byte[dim][];
			for (var i = 0; i < dim; i++) {
				result[i] = new byte[data.GetSize(i)];
				data[..1]
			}
		}*/
	}
}
