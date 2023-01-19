using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using IronPython.Hosting;
using IronPython.Runtime;
using Microsoft.Psi;
using Microsoft.Scripting;
using Microsoft.Scripting.Hosting;
using Microsoft.Scripting.Runtime;
using OpenSense.Components.Contract;

namespace OpenSense.Components.PythonSupports {
    [Serializable]
    public sealed class PythonConfiguration : OperatorConfiguration {

        private readonly ScriptEngine _engine;

        private Lazy<ScriptScope> metadataScope;

        #region Settings
        private string metadataCode = MetadataCodeTemplate;

        public string MetadataCode {
            get => metadataCode;
            set => SetProperty(ref metadataCode, value);
        }

        private string runtimeCode = RuntimeCodeTemplate;

        public string RuntimeCode {
            get => runtimeCode;
            set => SetProperty(ref runtimeCode, value);
        }
        #endregion

        private ScriptScope MetadataScope => metadataScope?.Value;

        public PythonConfiguration() {
            /** Create runtime 
             */
            var options = new Dictionary<string, object> {
                //{ "Debug", ScriptingRuntimeHelpers.True }, //TODO: make it become an option.
            };
            var runtime = Python.CreateRuntime(options);
            //Debug.Assert(runtime.Setup.DebugMode);

            /** Redirect outputs.
             * Need to be done before Passing it to Engine.
             */
            //Nothing need to be done here. The default behavior is redirecting to Console.
            //Note: IronPython3 has a bug that output redirection not working, so we cannot use print(). See https://github.com/IronLanguages/ironpython3/issues/1311.

            /** Add default assemblies
             */
            runtime.LoadAssembly(Assembly.GetAssembly(typeof(object)));
            runtime.LoadAssembly(Assembly.GetAssembly(typeof(Pipeline)));
            runtime.LoadAssembly(Assembly.GetAssembly(typeof(HelperExtensions)));
            runtime.LoadAssembly(Assembly.GetAssembly(typeof(PythonConfiguration)));

            /** Initialize engine.
             */

            _engine = Python.GetEngine(runtime);

            /** Add default paths
             */
            var paths = _engine.GetSearchPaths();
            //Nothing

            /** Initialize metadata code scope.
             */
            ResetMetadataEnvironment();

            /** Listen to code changes.
             */
            PropertyChanged += OnSelfPropertyChanged;
        }

        #region ComponentConfiguration
        public override IComponentMetadata GetMetadata() => new PythonMetadata(
            getPortsCallback: () => ComponentPorts
        );

        public override object Instantiate(Pipeline pipeline, IReadOnlyList<ComponentEnvironment> instantiatedComponents, IServiceProvider serviceProvider) {
            var result = new PythonRuntimeObject(pipeline, _engine, RuntimeCode, ComponentPorts);

            /** Connect
             */
            foreach (var inputConfig in Inputs) {
                var inputMetadata = (PythonPortMetadata)this.FindPortMetadata(inputConfig.LocalPort);
                Debug.Assert(inputMetadata.Direction == PortDirection.Input);
                dynamic consumer = result.Consumers[inputConfig.LocalPort.Identifier];

                var remoteEnvironment = instantiatedComponents.Single(e => inputConfig.RemoteId == e.Configuration.Id);
                var remoteOutputMetadata = remoteEnvironment.FindPortMetadata(inputConfig.RemotePort);
                Debug.Assert(remoteOutputMetadata.Direction == PortDirection.Output);
                var getProducerFunc = typeof(HelperExtensions)
                    .GetMethod(nameof(HelperExtensions.GetProducer))
                    .MakeGenericMethod(inputMetadata.TransmissionDataType);
                dynamic producer = getProducerFunc.Invoke(null, new object[] { remoteEnvironment, inputConfig.RemotePort });

                Operators.PipeTo(producer, consumer, inputConfig.DeliveryPolicy);
            }
            return result;
        }
        #endregion

        #region Event Listeners
        private void OnSelfPropertyChanged(object sender, PropertyChangedEventArgs args) {
            switch (args.PropertyName) {
                case nameof(MetadataCode):
                    ResetMetadataEnvironment();
                    break;
            }
        }
        #endregion

        #region IComponentMetadata

        private Lazy<IReadOnlyList<PythonPortMetadata>> componentPorts;

        private IReadOnlyList<PythonPortMetadata> ComponentPorts => componentPorts.Value;

        #endregion

        private void ResetMetadataEnvironment() {
            metadataScope = new Lazy<ScriptScope>(() => {
                var result = CreateScope(_engine, MetadataCode);
                return result;
            });

            componentPorts = new Lazy<IReadOnlyList<PythonPortMetadata>>(() => {
                var result = GetComponentPorts(_engine, MetadataScope);
                return result;
            });
        }

        private static ScriptScope CreateScope(ScriptEngine engine, string code) {
            var result = engine.CreateScope(/*dict*/);

            /** Add default variables here
             */
            //None

            /** Run code
             */
            if (string.IsNullOrEmpty(code)) {
                return result;
            }
            try {
                var source = engine.CreateScriptSourceFromString(code);
                var compiled = source.Compile();
                _ = compiled.Execute(result);
            } catch (SyntaxErrorException ex) {
                var message = engine.GetService<ExceptionOperations>().FormatException(ex);
                throw new Exception(message, ex);
            }
            return result;
        }

        private static IReadOnlyList<PythonPortMetadata> GetComponentPorts(ScriptEngine engine, ScriptScope metadataScope) {
            var result = new List<PythonPortMetadata>();

            if (metadataScope.TryGetVariable("PORTS", out var portsRaw)) {
                if (engine.Operations.TryConvertTo<PythonList>(portsRaw, out PythonList ports)) {
                    foreach (var portRaw in ports) {
                        if (portRaw is PythonPortMetadata port) {
                            result.Add(port);
                        }
                    }
                }

            }

            return result;
        }

        #region Code Templates
        private const string MetadataCodeTemplate = @"# Define ports in PORTS list variable.
# (TO BE RESOLVED) Better error handling when editing codes.
# (TO BE RESOLVED) Ports will be visiable after reloading the UI control.
# (TO BE RESOLVED) Read PORTS as iterator.
# (TO BE RESOLVED) Provide helper methods for creating array inputs.
import OpenSense.Components.PythonSupports.PortBuilder as pb
PORTS = [
    pb.Create().AsInput().WithName(""In"").WithType(float).Build(), # Python's float is mapped to .NET's System.Double.
    pb.Create().AsOutput().WithName(""Out"").WithType(int).Build(), # Python's int is mapped to .NET's System.Numerics.BigInteger.
]
";

        private const string RuntimeCodeTemplate = @"# Define functions for inputs.
# (TO BE RESOLVED) A runtime code and ports visualization.
def In(value, envelope): # Parameter envelope is of type Microsoft.Psi.Envelope.
    val = int(value)
    Out.Post(val, envelope.OriginatingTime)
";
        #endregion
    }
}
