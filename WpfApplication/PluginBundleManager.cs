#nullable enable

using System;
using System.Collections.Generic;
using System.Composition;
using System.Composition.Hosting;
using System.IO;
using System.Linq;
using System.Reflection;
using Microsoft.Extensions.Logging;

namespace OpenSense.WPF {
    internal sealed class PluginBundleManager {

        private const string PluginDirectory = "plugins";//This path is also defined in the csproj file.

        private readonly List<PluginBundle> pluginBundles = new();

        private readonly ILogger? _logger;

        private static readonly Lazy<PluginBundleManager> _default = new(() => new (Path.Combine(AppDomain.CurrentDomain.BaseDirectory, PluginDirectory), null));

        public static PluginBundleManager Default => _default.Value;//TODO: use DI

        public PluginBundleManager(string pluginDirectory, ILogger? logger = null) {
            _logger = logger;

            /* Load Plugins */
            if (!Directory.Exists(pluginDirectory)) {
                _logger?.LogWarning("Plugin directory not found. It should be located at \"{pluginDirectory}\".", pluginDirectory);
            } else {
                var bundleDirectories = Directory.EnumerateDirectories(pluginDirectory);
                foreach (var bundleDirectory in bundleDirectories) {
                    var directoryInfo = new DirectoryInfo(bundleDirectory);
                    var loadContext = new PluginBundleLoadContext(directoryInfo.Name, bundleDirectory, _logger);
                    var loadedAssemblies = loadContext.LoadAssemblies(bundleDirectory).ToArray();
                    if (loadedAssemblies.Length == 0) {
                        _logger?.LogWarning("No assembly found in plugin bundle folder \"{folderName}\".", directoryInfo.Name);
                    } else {
                        var bundle = new PluginBundle(bundleDirectory, loadContext, loadedAssemblies);
                        pluginBundles.Add(bundle);
                    }
                }
                if (pluginBundles.Count == 0) {
                    _logger?.LogWarning("No plugin bundle found in \"{pluginDirectory}\".", pluginDirectory);
                }
            }
        }

        public void SatisfyImports(object objectWithLooseImports) {
            var assemblies = pluginBundles.SelectMany(b => b.InitialAssemblies).ToArray();
            var configuration = new ContainerConfiguration()
                .WithAssemblies(assemblies);//note: Fluent interface
            using var container = configuration.CreateContainer();
            container.SatisfyImports(objectWithLooseImports);
        }

        #region Classes
        private sealed class PluginBundle {

            public string FolderName { get; }

            public PluginBundleLoadContext LoadContext { get; }

            public IReadOnlyCollection<Assembly> InitialAssemblies { get; }

            public PluginBundle(string folderName, PluginBundleLoadContext loadContext, IReadOnlyCollection<Assembly> initialAssemblies) {
                LoadContext = loadContext;
                FolderName = folderName;
                InitialAssemblies = initialAssemblies;
            }

        }
        #endregion
    }
}
