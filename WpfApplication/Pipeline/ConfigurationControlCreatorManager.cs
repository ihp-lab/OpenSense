﻿using System;
using System.Collections.Generic;
using System.Composition;
using System.Composition.Hosting;
using System.Linq;
using System.Reflection;
using System.Windows;
using OpenSense.Components;
using OpenSense.WPF.Components;

namespace OpenSense.WPF.Pipeline {
    internal sealed class ConfigurationControlCreatorManager {

        [ImportMany]
        private IConfigurationControlCreator[] creators { get; set; }

        public ConfigurationControlCreatorManager() {
            var assemblies = new List<Assembly>() {
                typeof(ConfigurationControlCreatorManager).Assembly,
                Assembly.GetEntryAssembly(),
            };
            assemblies.AddRange(PluginAssemblyLoadContext.LoadAssemblies(AppDomain.CurrentDomain.BaseDirectory));
            var configuration = new ContainerConfiguration()
                .WithAssemblies(assemblies);//note: Fluent interface
            using var container = configuration.CreateContainer();
            container.SatisfyImports(this);
        }

        public UIElement Create(ComponentConfiguration configuration) {
            var creator = creators.FirstOrDefault(c => c.CanCreate(configuration));
            return creator?.Create(configuration);
        }
    }
}
