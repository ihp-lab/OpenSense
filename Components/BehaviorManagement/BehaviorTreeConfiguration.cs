using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Microsoft.Psi;

namespace OpenSense.Components.BehaviorManagement {
    [Serializable]
    public class BehaviorTreeConfiguration : ComponentConfiguration {

        private BehaviorRuleConfiguration root = new MimicRuleConfiguration();

        public BehaviorRuleConfiguration Root {
            get => root;
            set => SetProperty(ref root, value);
        }

        #region ComponentConfiguration
        public override IComponentMetadata GetMetadata() => 
            new BehaviorTreeMetadata(this);

        public override object Instantiate(Pipeline pipeline, IReadOnlyList<ComponentEnvironment> instantiatedComponents, IServiceProvider? serviceProvider) {
            var result = new BehaviorTree(pipeline, Root, serviceProvider, DeliveryPolicy.Unlimited);
            foreach (var inputConfig in Inputs) {
                var inputMetadata = this.FindPortMetadata(inputConfig.LocalPort);
                var connectorInfo = result.GetConnectorInfo(inputMetadata);

                Debug.Assert(inputMetadata.Direction == PortDirection.Input);
                var key = (string)inputConfig.LocalPort.Identifier;
                dynamic consumer = connectorInfo.Connector;

                var remoteEnvironment = instantiatedComponents.Single(e => inputConfig.RemoteId == e.Configuration.Id);
                var remoteOutputMetadata = remoteEnvironment.FindPortMetadata(inputConfig.RemotePort);
                Debug.Assert(remoteOutputMetadata.Direction == PortDirection.Output);
                var getProducerFunc = typeof(HelperExtensions)
                    .GetMethod(nameof(HelperExtensions.GetProducer))!
                    .MakeGenericMethod(connectorInfo.DataType);
                dynamic producer = getProducerFunc.Invoke(null, new object[] { remoteEnvironment, inputConfig.RemotePort })!;

                Operators.PipeTo(producer, consumer, inputConfig.DeliveryPolicy);
            }
            return result;
        }
        #endregion
    }
}
