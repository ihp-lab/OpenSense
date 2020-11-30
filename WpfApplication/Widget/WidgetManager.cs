using System;
using System.Collections.Generic;
using System.Composition;
using System.Composition.Hosting;
using System.IO;
using System.Reflection;
using OpenSense.Wpf.Widget.Contract;

namespace OpenSense.Wpf.Widget {
    public class WidgetManager {

        [ImportMany]
        private IWidgetMetadata[] weidgets { get; set; }

        public IReadOnlyList<IWidgetMetadata> Widgets => weidgets;

        public WidgetManager() {
            var assemblies = new List<Assembly>() {
                typeof(WidgetManager).Assembly,
                Assembly.GetEntryAssembly(),
            };
            //filter dll name with "Widget". Otherwise, when loading OpenFaceInterop, an exception from (Diagnose Errors with Managed Debugging Assistants) will be thrown
            var files = Directory.EnumerateFiles(AppDomain.CurrentDomain.BaseDirectory, "*Widget*.dll");
            foreach (var file in files) {
                try {
                    var asm = Assembly.LoadFrom(file);
                    assemblies.Add(asm);
                } catch (BadImageFormatException) {

                }
            }
            var configuration = new ContainerConfiguration()
                .WithAssemblies(assemblies);//note: Fluent interface
            using var container = configuration.CreateContainer();
            container.SatisfyImports(this);
        }
    }
}
