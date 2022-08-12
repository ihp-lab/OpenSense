using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Microsoft.Psi;
using OpenSense.Component.Contract;

namespace OpenSense.Component.Psi {
    [Serializable]
    public abstract class FusionConfiguration : ComponentConfiguration {

        protected IReadOnlyList<dynamic> GetRemoteProducers(IReadOnlyList<ComponentEnvironment> instantiatedComponents) {
            var configurations = instantiatedComponents.Select(e => e.Configuration).ToArray();
            var result = new List<dynamic>();
            foreach (var inputConfig in Inputs) {
                var remoteEnv = instantiatedComponents.Single(env => env.Configuration.Id == inputConfig.RemoteId);
                var remotePortMeta = remoteEnv.Configuration.GetMetadata().FindPortMetadata(inputConfig.RemotePort);
                var remoteDataType = remoteEnv.Configuration.FindOutputPortDataType(remotePortMeta, configurations);
                Debug.Assert(remoteDataType != null);
                var getRemoteProducerFunc = typeof(HelperExtensions).GetMethod(nameof(HelperExtensions.GetProducer)).MakeGenericMethod(remoteDataType);
                dynamic producer = getRemoteProducerFunc.Invoke(null, new object[] { remoteEnv, inputConfig.RemotePort });
                result.Add(producer);
            }
            Debug.Assert(result.Count == Inputs.Count);
            return result;
        }
    }
}
