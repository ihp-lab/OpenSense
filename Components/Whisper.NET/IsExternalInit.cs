// Polyfill for init-only / record support on netstandard2.0 (.NET 5+ ships this type).
// Guarded so multi-targeting newer frameworks doesn't produce a duplicate definition.
#if NETSTANDARD2_0
namespace System.Runtime.CompilerServices {

    internal static class IsExternalInit { }
}
#endif
