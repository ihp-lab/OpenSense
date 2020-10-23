using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenSense.Utilities {
    public static class OpenUrl {
        public static void Open(string url) {
            var psi = new ProcessStartInfo {
                FileName = url,
                UseShellExecute = true
            };
            try {
                Process.Start(psi);
            } catch (Win32Exception) {

            }
        }
    }
}
