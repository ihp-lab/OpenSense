using System.Runtime.InteropServices;

namespace OpenSense.Components.ParallelPorts {
    /// <summary>
    /// Inpoutx64 methods.
    /// </summary>
    /// <remarks>
    /// Other methods are provided for compatibility with APIs other than Inpout32, but they are not used here.
    /// </remarks>
    internal static class Inpoutx64PInvoke {
        [DllImport("inpoutx64.dll", EntryPoint = "Out32")]
        public static extern void Out32(short PortAddress, short Data);

        [DllImport("inpoutx64.dll", EntryPoint = "Inp32")]
        public static extern short Inp32(short PortAddress);

        [DllImport("inpoutx64.dll", EntryPoint = "IsInpOutDriverOpen")]
        public static extern bool IsInpOutDriverOpen();

        [DllImport("inpoutx64.dll", EntryPoint = "IsXP64Bit")]
        public static extern bool IsXP64Bit();
    }
}
