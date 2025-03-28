namespace OpenSense.WPF.Components.AzureKinect.Sensor {
    /// <summary>
    /// The designer does not work if we use our common method to reference an enum in XAML.
    /// The project still compiles, just the designer does not work, it cannot find the type. No matter I specify assembly=System or System.Runtime.
    /// Don't know why.
    /// So this is a workaround.
    /// </summary>
    internal static class ThreadPriorityEnumHelper {

        public static ThreadPriority[] Values { get; } = (ThreadPriority[])Enum.GetValues(typeof(ThreadPriority));
    }
}
