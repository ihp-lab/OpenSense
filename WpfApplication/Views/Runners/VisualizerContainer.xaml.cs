#nullable enable

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows.Controls;
using AvalonDock.Layout;
using OpenSense.Components;
using OpenSense.Pipeline;
using OpenSense.WPF.Pipeline;

namespace OpenSense.WPF.Views.Runners {
    public sealed partial class VisualizerContainer : UserControl {

        public VisualizerContainer() {
            InitializeComponent();
        }

        public void Setup(PipelineEnvironment environment) {
            RootLayoutPanel.Children.Clear();
            var map = new Dictionary<Guid, LayoutDocumentPane>(environment.Instances.Count);
            var creators = new InstanceControlCreatorManager();
            foreach (var component in environment.Instances) {
                var control = CreateControl(component, creators);
                map.Add(component.Configuration.Id, control);
            }
            var instantiated = new List<Guid>(environment.Instances.Count);
            var notInstantiated = new HashSet<ComponentConfiguration>(environment.Instances.Select(e => e.Configuration));
            while (notInstantiated.Count > 0) {
                var component = notInstantiated.First(c => IsReadyToInstantiate(c, instantiated));
                notInstantiated.Remove(component);
                instantiated.Add(component.Id);
                var parentIds = component.Inputs.Select(i => i.RemoteId);
                var parent = parentIds
                    .Select(i => (Id: i, Control: map[i]))
                    .GroupBy(t => CountHeight(t.Control)).OrderBy(g => g.Key).FirstOrDefault()//Select smaller height parent
                    ?.GroupBy(t => CountWidth(t.Control)).OrderBy(g => g.Key).FirstOrDefault()//Select smaller width parent
                    ?.FirstOrDefault()
                    .Control;//Select one of results
                var control = map[component.Id];
                AddToLayout(control, parent);
            }
        }

        public void ConsoleDump() {
            RootLayoutPanel.ConsoleDump(0);
        }

        #region Helpers
        private static LayoutDocumentPane CreateControl(ComponentEnvironment environment, InstanceControlCreatorManager creators) {
            var control = creators.Create(environment.Instance);//DataContext should be set inside this method.
            control ??= new EmptyControl();
            var anchorable = new LayoutDocument() {
                Content = control,
                Title = environment.Configuration.Name,
            };
            var result = new LayoutDocumentPane(anchorable);
            return result;
        }

        private static bool IsReadyToInstantiate(ComponentConfiguration config, IReadOnlyList<Guid> instantiated) {
            var result = config.Inputs.All(i => instantiated.Contains(i.RemoteId));
            return result;
        }

        private int CountHeight(LayoutDocumentPane control) {
            var result = 0;
            var current = (ILayoutElement)control;
            while (current.Parent != RootLayoutPanel) {
                current = current.Parent;
                result++;
            }
            return result;
        }

        private int CountWidth(LayoutDocumentPane control) {
            var parentPanel = (LayoutPanel)control.Parent;
            if (parentPanel.Orientation == Orientation.Horizontal) {//No child
                return 0;
            }
            var childrenPanel = (LayoutPanel)parentPanel.Children[1];
            Debug.Assert(childrenPanel.Orientation == Orientation.Horizontal);
            return childrenPanel.Children.Count;
        }

        private void AddToLayout(LayoutDocumentPane control, LayoutDocumentPane? parent) {
            if (parent is null) {
                Debug.Assert(RootLayoutPanel.Orientation == Orientation.Horizontal);
                RootLayoutPanel.InsertChildAt(RootLayoutPanel.Children.Count, control);
                Debug.Assert(control.Parent == RootLayoutPanel);
                return;
            }
            var parentPanel = (LayoutPanel)parent.Parent;
            if (parentPanel.Orientation == Orientation.Horizontal) {//First child
                var name = parent.Children.Single().Title;
                var hierarchyPanel = new TaggedLayoutPanel($"{name} - Hierarchy") {
                    Orientation = Orientation.Vertical,
                };
                parentPanel.ReplaceChild(parent, hierarchyPanel);
                Debug.Assert(hierarchyPanel.Parent == parentPanel);
                hierarchyPanel.InsertChildAt(0, parent);
                Debug.Assert(parent.Parent == hierarchyPanel);
                var childrenPanel = new TaggedLayoutPanel($"{name} - Children") { 
                    Orientation = Orientation.Horizontal,
                };
                hierarchyPanel.InsertChildAt(1, childrenPanel);
                Debug.Assert(childrenPanel.Parent == hierarchyPanel);
                childrenPanel.InsertChildAt(0, control);
                Debug.Assert(control.Parent == childrenPanel);
            } else {
                var childrenPanel = (LayoutPanel)parentPanel.Children[1];
                Debug.Assert(childrenPanel.Orientation == Orientation.Horizontal);
                childrenPanel.InsertChildAt(childrenPanel.Children.Count, control);
                Debug.Assert(control.Parent == childrenPanel);
            }
        }
        #endregion
    }
}
