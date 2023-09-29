using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace OpenSense.Components.BehaviorManagement {

    internal abstract class CompositeRule : IBehaviorRule, IReadOnlyList<IBehaviorRule>, IDisposable {

        protected readonly IReadOnlyList<IBehaviorRule> _children;

        public CompositeRule(IReadOnlyList<IBehaviorRule> children) {
            _children = children;
        }

        #region IBehaviorRule
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
