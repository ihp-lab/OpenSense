#nullable enable

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows;
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

        public void Clear() {
            RootLayoutPanel.Children.Clear();
        }

        public void Setup(PipelineEnvironment environment) {
            RootLayoutPanel.Children.Clear();
            var map = new Dictionary<Guid, TaggedLayoutDocumentPane>(environment.Instances.Count);
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
                    .GroupBy(t => CountHeight(t.Control)).OrderByDescending(g => g.Key).FirstOrDefault()//Select greater height parent
                    ?.GroupBy(t => CountWidth(t.Control)).OrderBy(g => g.Key).FirstOrDefault()//Select smaller width parent
                    ?.FirstOrDefault()
                    .Control;//Select one of results
                var control = map[component.Id];
                AddToLayout(control, parent);
            }
            SimplifyLayouts(RootLayoutPanel);
            BindEventHandlers(RootLayoutPanel);
        }

        public void SimplifyLayouts() {
            UnbindEventHandlers(RootLayoutPanel);
            SimplifyLayouts(RootLayoutPanel);
            BindEventHandlers(RootLayoutPanel);
        }

        public void RemoveEmptyControls() {
            UnbindEventHandlers(RootLayoutPanel);
            RootLayoutPanel
                .Descendents()
                .OfType<LayoutDocument>()
                .Where(d => d.Content is EmptyControl)
                .Select(d => (LayoutDocumentPane)d.Parent)
                .ToList()
                .ForEach(p => p.Parent?.RemoveChild(p));
            SimplifyLayouts(RootLayoutPanel);
            BindEventHandlers(RootLayoutPanel);
        }

        #region Helpers
        private static TaggedLayoutDocumentPane CreateControl(ComponentEnvironment environment, InstanceControlCreatorManager creators) {
            var name = environment.Configuration.Name;
            var control = creators.Create(environment.Instance);//DataContext should be set inside this method.
            control ??= new EmptyControl();
            var anchorable = new LayoutDocument() {
                Content = control,
                Title = name,
            };
            var result = new TaggedLayoutDocumentPane(anchorable, name) { 
                DockMinHeight = 10,
                DockMinWidth = 10,
                DockHeight = new GridLength(1, GridUnitType.Star),
                DockWidth = new GridLength(1, GridUnitType.Star),
                ResizableAbsoluteDockWidth = 1,
                ResizableAbsoluteDockHeight = 1,
            };
            return result;
        }

        private static bool IsReadyToInstantiate(ComponentConfiguration config, IReadOnlyList<Guid> instantiated) {
            var result = config.Inputs.All(i => instantiated.Contains(i.RemoteId));
            return result;
        }

        private int CountHeight(TaggedLayoutDocumentPane control) {
            var result = 0;
            var current = (ILayoutElement)control;
            while (current.Parent != RootLayoutPanel) {
                current = current.Parent;
                result++;
            }
            return result;
        }

        private int CountWidth(TaggedLayoutDocumentPane control) {
            var parentPanel = (LayoutPanel)control.Parent;
            if (parentPanel.Orientation == Orientation.Horizontal) {//No child
                return 0;
            }
            var childrenPanel = (LayoutPanel)parentPanel.Children[1];
            Debug.Assert(childrenPanel.Orientation == Orientation.Horizontal);
            return childrenPanel.Children.Count;
        }

        private void AddToLayout(TaggedLayoutDocumentPane control, TaggedLayoutDocumentPane? parent) {
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
                    DockHeight = new GridLength(1, GridUnitType.Star),
                    DockWidth = new GridLength(1, GridUnitType.Star),
                    DockMinHeight = 10,
                    DockMinWidth = 10,
                    ResizableAbsoluteDockWidth = 1,
                    ResizableAbsoluteDockHeight = 1,
                };
                parentPanel.ReplaceChild(parent, hierarchyPanel);
                Debug.Assert(parent.Parent is null);
                Debug.Assert(hierarchyPanel.Parent == parentPanel);
                hierarchyPanel.InsertChildAt(0, parent);
                Debug.Assert(parent.Parent == hierarchyPanel);
                var childrenPanel = new TaggedLayoutPanel($"{name} - Children") { 
                    Orientation = Orientation.Horizontal,
                    DockHeight = new GridLength(1, GridUnitType.Star),
                    DockWidth = new GridLength(1, GridUnitType.Star),
                    DockMinHeight = 10,
                    DockMinWidth = 10,
                    ResizableAbsoluteDockWidth = 1,
                    ResizableAbsoluteDockHeight = 1,
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

        /// <remarks>Without this method, some controls may not be visible.</remarks>
        private bool SimplifyLayouts(ILayoutElement elem) {
            var result = false;

            /* Simplify children first */
            if (elem is LayoutPanel panel) {
                var anyModified = true;//Since the collection may be modified while iterating, we do not use foreach.
                while (anyModified) {
                    anyModified = panel.Children.Any(SimplifyLayouts);
                    if (anyModified) {
                        result = true;
                    }
                }
            }

            /* If this element is the only child of its parent, then remove its parnet. */
            var toBeRemoved = elem.Parent;
            if (toBeRemoved?.ChildrenCount == 1 && toBeRemoved.Parent is not null) {
                if (toBeRemoved.Parent is not LayoutRoot) {//Do not remove the top most panel which is not generated by code.
                    toBeRemoved.Parent.ReplaceChild(toBeRemoved, elem);
                    result = true;
                }
            }

            /* If the element's orientation is the same as the parent's orientation, then remove the element. */
            if (elem is LayoutPanel panel2) {
                var toBeMerged = panel2.Parent as LayoutPanel;
                if (panel2.Orientation == toBeMerged?.Orientation) {
                    var insertIndex = toBeMerged.IndexOfChild(panel2);
                    toBeMerged.RemoveChildAt(insertIndex);
                    while (panel2.ChildrenCount > 0) {
                        var lastChild = panel2.Children[panel2.ChildrenCount - 1];
                        panel2.RemoveChild(lastChild);
                        toBeMerged.InsertChildAt(insertIndex, lastChild);
                    }
                    result = true;
                }
            }

            return result;
        }

        private void BindEventHandlers(ILayoutElement element) {
            element
                .Descendents()
                .OfType<TaggedLayoutPanel>()
                .ToList()
                .ForEach(p => p.ChildrenCollectionChanged += OnChildrenCollectionChanged);
        }

        private void UnbindEventHandlers(ILayoutElement element) {
            element
                .Descendents()
                .OfType<TaggedLayoutPanel>()
                .ToList()
                .ForEach(p => p.ChildrenCollectionChanged -= OnChildrenCollectionChanged);
        }
        #endregion

        #region Layout Event Handlers
        private void OnChildrenCollectionChanged(object? sender, EventArgs args) {
            Debug.Assert(sender is not null);
            var panel = (TaggedLayoutPanel)sender;
            switch (panel.ChildrenCount) {
                case 0:
                    panel.ChildrenCollectionChanged -= OnChildrenCollectionChanged;//avoid recursive call
                    panel.Parent.RemoveChild(panel);
                    break;
                case 1:
                    panel.ChildrenCollectionChanged -= OnChildrenCollectionChanged;//avoid recursive call
                    var child = panel.Children.Single();
                    panel.Parent.ReplaceChild(panel, child);
                    break;
            }
        }
        #endregion
    }
}
