using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using Microsoft.Extensions.Logging;
using Microsoft.Psi;
using Microsoft.Scripting;
using Microsoft.Scripting.Hosting;

namespace OpenSense.Components.PythonSupports {
    public sealed class PythonRuntimeObject : IDisposable, INotifyPropertyChanged {

        private const string DisposeMethodName = "Dispose";
        private const string LoggerVariableName = "Logger";

        private readonly ScriptEngine _engine;
        private readonly ScriptScope _scope;
        private readonly Dictionary<object, object> _receivers = new Dictionary<object, object>();
        private readonly Dictionary<object, object> _emitters = new Dictionary<object, object>();

        public IReadOnlyDictionary<object, object> Producers => _emitters;

        public IReadOnlyDictionary<object, object> Consumers => _receivers;

        internal PythonRuntimeObject(
            Pipeline pipeline, 
            ScriptEngine engine, 
            string code, 
            IReadOnlyList<StaticPortMetadata> ports, 
            ILogger pythonLogger
            ) {
            _engine = engine;
            _scope = _engine.CreateScope(/*dict*/);

            /** Add default variables here
             */
            if (pythonLogger is not null) {
                _scope.SetVariable(LoggerVariableName, pythonLogger);
            }

            /** Add producers
             */
            foreach (var portMetadata in ports) {
                switch (portMetadata.Direction) {
                    case PortDirection.Input:
                        break;
                    case PortDirection.Output:
                        var createMethod = typeof(Pipeline)
                            .GetMethod(nameof(Pipeline.CreateEmitter))
                            .MakeGenericMethod(portMetadata.DataType);
                        var emitter = createMethod.Invoke(pipeline, new object[] { 
                            this, 
                            portMetadata.Name,
                            null, //messageValidator
                        });
                        _emitters[portMetadata.Identifier] = emitter;
                        _scope.SetVariable(portMetadata.Identifier.ToString(), emitter);
                        break;
                    default:
                        throw new InvalidOperationException();
                }
            }

            /** Execute code
             */
            if (!string.IsNullOrEmpty(code)) {
                try {
                    var source = engine.CreateScriptSourceFromString(code);
                    var compiled = source.Compile();
                    _ = compiled.Execute(_scope);
                } catch (SyntaxErrorException ex) {
                    var message = engine.GetService<ExceptionOperations>().FormatException(ex);
                    throw new Exception(message, ex);
                }
            }

            /** Add receivers
             */
            foreach (var portMetadata in ports) {
                switch (portMetadata.Direction) {
                    case PortDirection.Output:
                        break;
                    case PortDirection.Input:
                        var identifier = portMetadata.Identifier.ToString();
                        var processingMethodType = typeof(Action<,>).MakeGenericType(portMetadata.DataType, typeof(Envelope));
                        if (!_scope.TryGetVariable(identifier, out var delegateRaw)) {
                            throw new InvalidOperationException($"Did not find a \"{identifier}\" function in runtime Python code.");
                        }
                        if (!engine.Operations.TryConvertTo(delegateRaw, processingMethodType, out dynamic @delegate)) {
                            throw new InvalidOperationException($"The \"{identifier}\" function's argument list does not match.");
                        }

                        var createMethod = typeof(Pipeline)
                            .GetMethods(BindingFlags.Instance | BindingFlags.Public)
                            .Where(m => m.Name == nameof(Pipeline.CreateReceiver))
                            .Single(m => m.GetParameters()[1].ParameterType.GetGenericTypeDefinition() == typeof(Action<,>))
                            .MakeGenericMethod(portMetadata.DataType);
                        var receiver = createMethod.Invoke(pipeline, new object[] {
                            this,
                            @delegate,
                            portMetadata.Name,
                        });
                        _receivers[portMetadata.Identifier] = receiver;
                        break;
                    default:
                        throw new InvalidOperationException();
                }
            }
        }

        #region INotifyPropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;

        private void SetProperty<T>(ref T field, T value, [CallerMemberName] string propertyName = null) {
            if (!EqualityComparer<T>.Default.Equals(field, value)) {
                field = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            }
        }
        #endregion

        #region IDisposable
        private bool disposed;

        public void Dispose() {
            if (disposed) {
                return;
            }

            if (_scope.TryGetVariable<Action>(DisposeMethodName, out var callable)) {
                callable();
            }

            disposed = true;
        }
        #endregion
    }
}
