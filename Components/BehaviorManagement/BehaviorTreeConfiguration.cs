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

        private DeliveryPolicy deliveryPolicy = DeliveryPolicy.LatestMessage;

        public DeliveryPolicy DeliveryPolicy {
            get => deliveryPolicy;
            set => SetProperty(ref deliveryPolicy, value);
        }

        #region ComponentConfiguration
        public override IComponentMetadata GetMetadata() => 
            new BehaviorTreeMetadata(this);

        public override object Instantiate(Pipeline pipeline, IReadOnlyList<ComponentEnvironment> instantiatedComponents, IServiceProvider? serviceProvider) {
            /* Instantiate rule */
            var rule = Root.Instantiate(serviceProvider);

            /* Inference port data types */
            var metadata = GetMetadata();
            var ports = new List<BehaviorTree.PortInfo>();
            var configurations = instantiatedComponents.Select(e => e.Configuration).ToArray();
            foreach (var port in metadata.Ports) {
                Type? type;
                DeliveryPolicy? deliveryPolicy;
                switch (port.Direction) {
                    case PortDirection.Input:
                        type = this.FindInputPortDataType(port, configurations);
                        var inputConfig = Inputs.SingleOrDefault(i => Equals(i.LocalPort.Identifier, port.Identifier));
                        deliveryPolicy = inputConfig?.DeliveryPolicy;
                        break;
                    case PortDirection.Output:
                        type = this.FindOutputPortDataType(port, configurations);
                        deliveryPolicy = null;
                        break;
                    default:
                        throw new InvalidOperationException();
                }
                ports.Add(new BehaviorTree.PortInfo(port, type, deliveryPolicy));
            }

            /* Instantiate subpipeline */
            var result = new BehaviorTree(pipeline, rule, Root.Window, ports, DeliveryPolicy);

            /* Connect inputs */
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
