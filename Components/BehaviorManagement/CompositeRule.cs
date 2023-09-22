using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace OpenSense.Components.BehaviorManagement {

    internal abstract class CompositeRule : IBehaviorRule, IReadOnlyList<IBehaviorRule>, IDisposable {

        protected readonly IReadOnlyList<IBehaviorRule> _children;

        protected readonly IPortMetadata[] _ports;

        protected readonly TimeSpan _window;

        public CompositeRule(IReadOnlyList<IBehaviorRule> children) {
            _children = children;
            _ports = children
                .SelectMany(c => c.Ports)
                .ToArray()
                ;
            _window = children
                .Select(r => r.Window)
                .Aggregate(TimeSpan.Zero, (a, v) => a > v ? a : v)
                ;
        }

        #region IBehaviorRule
        public TimeSpan Window => _window;

        public IReadOnlyCollection<IPortMetadata> Ports => _ports;

        public abstract ValueTask<BehaviorRuleResponse> EvaluateAsync(BehaviorRequest request, CancellationToken cancellationToken = default);
        #endregion

        #region IDisposable
        private bool disposed;

        public virtual void Dispose() {
            if (disposed) {
                return;
            }
            disposed = true;

            foreach (var child in _children.OfType<IDisposable>()) {
                child.Dispose();
            }
        }
        #endregion

        #region IReadOnlyList
        public IBehaviorRule this[int index] => _children[index];

        public int Count => _children.Count;

        public IEnumerator<IBehaviorRule> GetEnumerator() => _children.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => _children.GetEnumerator();
        #endregion
    }
}
