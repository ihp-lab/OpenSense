using System;
using System.Collections.Generic;
using System.Linq;

namespace OpenSense.Components.BehaviorManagement {

    public readonly struct BehaviorRequest {

        public DateTime Time { get; }

        public TimeSpan Window { get; }

        public IReadOnlyCollection<BehaviorInputData> Inputs { get; }

        public BehaviorRequest(DateTime time, TimeSpan window, IReadOnlyCollection<BehaviorInputData> inputs) {
            Time = time;
            Window = window;
            Inputs = inputs;
        }

        #region Helpers
        private BehaviorInputData? GetData(IPortMetadata port) {
            var result = Inputs
                .Where(i => i.Port.Equals(port))
                .Select(i => (BehaviorInputData?)i)
                .SingleOrDefault();
            return result;
        }

        public BehaviorInputData? this[IPortMetadata port] => GetData(port);
        #endregion
    }
}
