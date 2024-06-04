#nullable enable

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Reflection.Metadata;
using System.Reflection.PortableExecutable;
using System.Runtime.Loader;
using Microsoft.Extensions.Logging;

namespace OpenSense.WPF {
    internal sealed class PluginBundleLoadContext : AssemblyLoadContext {

        private readonly List<AssemblyDependencyResolver> _resolvers = new();

        private readonly ILogger? _logger;

        public PluginBundleLoadContext(string name, string bundlePath, ILogger? logger = null) : base(name, isCollectible: false) {
            _logger = logger;
        }

        #region AssemblyLoadContext
        protected override Assembly? Load(AssemblyName assemblyName) {
            /* Check Duplication */
            /* NOTE: 
             * If this assembly exists in the Default context, then use that assembly and do not load a new one at here.
             * Otherwise, exceptions like
             *     System.Reflection.ReflectionTypeLoadException: 'Unable to load one or more of the requested types.
             *     Method 'MethodName' in type 'TypeName' from assembly 'AssemblyName' does not have an implementation.'
             * will be thrown at somewhere, because two different assemblies with the same name are referenced.
             * Returning null to fallback to the Default context.
             * Returning the assembly in the Default context will cause the exception as well.
             * This solution was found at https://stackoverflow.com/a/64955087/14067471.
             */
            Assembly? defaultAssembly = null;
            try {
                //Try to load an assembly with the same name to the Default context.
                defaultAssembly = Default.LoadFromAssemblyName(assemblyName);
            } catch (FileNotFoundException) {
                //Ignore
            }
#if DEBUG
            catch (Exception ex) {
                Debug.Fail(ex.Message);//Catch the derived exception types
            }
#endif
            if (defaultAssembly is not null) {
                var localPath = ResolveAssemblyToPath(assemblyName);
                if (localPath is not null) {
                    _logger?.LogInformation("Assembly \"{assemblyName}\" exists in the Default context. Assembly file \"{duplicateAssemblyFilePath}\" is not used.", assemblyName.Name, localPath);
                }
                return null;
            }

            var assemblyPath = ResolveAssemblyToPath(assemblyName);
            if (assemblyPath is not null) {
                var result = LoadFromAssemblyPath(assemblyPath);
                return result;
            }

            return null;
        }

        protected override nint LoadUnmanagedDll(string unmanagedDllName) {
            var assemblyPath = ResolveUnmanagedDllToPath(unmanagedDllName);
            if (assemblyPath is not null) {
                var result = LoadUnmanagedDllFromPath(assemblyPath);
                return result;
            }

            return nint.Zero;
        }
        #endregion

        #region APIs

        public IEnumerable<Assembly> LoadAssemblies(string rootPath) {
            _resolvers.Clear();
            var assemblyFiles = EnumerateAssemblyFiles(rootPath);
            foreach (var file in assemblyFiles) {
                var assemblyName = GetAssemblyName(file);
                var resolver = new AssemblyDependencyResolver(file);
                _resolvers.Add(resolver);

                /* Try to load assembly. */
                Assembly? asm = null;
                try {
                    /* NOTE:
                     * LoadFromAssemblyPath() will defer loading dependencies.
                     * LoadFromAssemblyName() will load dependencies immediately.
                     */
                    asm = LoadFromAssemblyName(assemblyName);
                } catch (BadImageFormatException) {
                    ;
                }
                if (asm is null) {
                    continue;
                }
                yield return asm;
            }
        }
        #endregion

        private static IEnumerable<string> EnumerateAssemblyFiles(string rootPath) {
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

        private string? ResolveAssemblyToPath(AssemblyName assemblyName) {
            foreach (var resolver in _resolvers) {
                var assemblyPath = resolver.ResolveAssemblyToPath(assemblyName);
                if (assemblyPath is not null) {
                    Debug.Assert(File.Exists(assemblyPath));
                    return assemblyPath;
                }
            }
            return null;
        }

        private string? ResolveUnmanagedDllToPath(string unmanagedDllName) {
            foreach (var resolver in _resolvers) {
                var assemblyPath = resolver.ResolveUnmanagedDllToPath(unmanagedDllName);
                if (assemblyPath is not null) {
                    Debug.Assert(File.Exists(assemblyPath));
                    return assemblyPath;
                }
            }
            return null;
        }
    }
}
