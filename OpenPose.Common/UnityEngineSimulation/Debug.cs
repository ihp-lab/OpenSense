using System;
using System.Collections.Generic;
using System.Text;

namespace UnityEngine {
	public static class Debug {

		public static void Log(string log) {
			Console.WriteLine(log);
		}

		public static void LogWarning(string log) {
			Console.WriteLine(log);
		}

		public static void LogError(string log) {
			Console.WriteLine(log);
		}

		public static void AssertFormat(bool value, string message) {
			if (!value) {
				throw new Exception(message);
			}
		}
	}
}
