using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Reflection.Metadata;
using System.Reflection.PortableExecutable;
using System.Runtime.Loader;

namespace OpenSense.WPF.Pipeline {
    internal sealed class PluginAssemblyLoadContext : AssemblyLoadContext {

        #region Methods to be moved
        internal static IEnumerable<string> EnumerateAssemblyFiles(string rootPath) {
            var files = Directory.EnumerateFiles(rootPath, "*.dll");
            foreach (var file in files) {
                var info = new FileInfo(file);
                Debug.Assert(file.Length > 0);
                if (info.Length == 0) {
                    continue;
                }

                /** Test if it is a valid .NET assembly without throwing any exception.
                 * Code from https://learn.microsoft.com/en-us/dotnet/standard/assembly/identify
                 */
                using (var fs = new FileStream(file, FileMode.Open, FileAccess.Read, FileShare.ReadWrite)) {
                    using var peReader = new PEReader(fs);
                    if (!peReader.HasMetadata) {
                        continue;
                    }
                    var reader = peReader.GetMetadataReader();
                    if (!reader.IsAssembly) {
                        continue;
                    }
                }

                yield return file;
            }
        }

        internal static IEnumerable<Assembly> LoadAssemblies(string rootPath) {
            foreach (var file in EnumerateAssemblyFiles(rootPath)) {
                /** Try to load assembly.
                  */
                Assembly asm = null;
                try {
                    asm = Assembly.LoadFrom(file);
                } catch (BadImageFormatException) {
                    ;
                }
                if (asm is null) {
                    continue;
                }
                yield return asm;
            }
        }
    }
    #endregion
}
