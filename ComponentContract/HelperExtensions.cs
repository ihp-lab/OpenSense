using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Reflection.Metadata;
using System.Reflection.PortableExecutable;
using Microsoft.Psi;

namespace OpenSense.Component.Contract {
    public static class HelperExtensions {

        public static IEnumerable<IPortMetadata> InputPorts(this IComponentMetadata componentMetadata) {
            return componentMetadata.Ports.Where(p => p.Direction == PortDirection.Input);
        }

        public static IEnumerable<IPortMetadata> OutputPorts(this IComponentMetadata componentMetadata) {
            return componentMetadata.Ports.Where(p => p.Direction == PortDirection.Output);
        }

        public static IPortMetadata FindPortMetadata(this IComponentMetadata componentMetadata, PortConfiguration port) {
            return componentMetadata.Ports.Single(p => Equals(p.Identifier, port.Identifier));
        }

        public static IPortMetadata FindPortMetadata(this ComponentConfiguration componentConfiguration, PortConfiguration port) {
            return componentConfiguration.GetMetadata().FindPortMetadata(port);
        }

        public static IPortMetadata FindPortMetadata(this ComponentEnvironment componentEnvironment, PortConfiguration port) {
            return componentEnvironment.Configuration.FindPortMetadata(port);
        }

        public static IProducer<T> GetStaticProducer<T>(this IComponentMetadata componentMetadata, object instance, PortConfiguration portConfiguration) {
            var portMetadata = componentMetadata.FindPortMetadata(portConfiguration);
            Debug.Assert(portMetadata.Direction == PortDirection.Output);
            Debug.Assert(portMetadata is StaticPortMetadata);
            var portStaticMetadata = (StaticPortMetadata)portMetadata;
            return portStaticMetadata.GetStaticProducer<T>(portConfiguration, instance);
        }

        private static object GetConnector<T>(this ComponentEnvironment componentEnvironment, PortConfiguration port) {
            return componentEnvironment.Configuration.GetMetadata().GetConnector<T>(componentEnvironment.Instance, port);
        }

        public static IProducer<T> GetProducer<T>(this ComponentEnvironment componentEnvironment, PortConfiguration port) {
            var connector = GetConnector<T>(componentEnvironment, port);
            return (IProducer<T>)connector;
        }

        public static IConsumer<T> GetConsumer<T>(this ComponentEnvironment componentEnvironment, PortConfiguration port) {
            var connector = GetConnector<T>(componentEnvironment, port);
            return (IConsumer<T>)connector;
        }

        private static dynamic GetStaticConnector(this object instance, StaticPortMetadata portMetadata, PortConfiguration portConfiguration) {
            Debug.Assert(Equals(portMetadata.Identifier, portConfiguration.Identifier));
            dynamic prop = portMetadata.Property.GetValue(instance);
            if (prop is null) {
                throw new Exception("instance connector is null");
            }
            switch (portMetadata.Aggregation) {
                case PortAggregation.Object:
                    return prop;
                case PortAggregation.List:
                    return prop[(int)portConfiguration.Index];
                case PortAggregation.Dictionary:
                    return prop[(string)portConfiguration.Index];
                default:
                    throw new InvalidOperationException();
            }
        }

        public static IProducer<T> GetStaticProducer<T>(this StaticPortMetadata portMetadata, PortConfiguration portConfiguration, object instance) {
            object obj = GetStaticConnector(instance, portMetadata, portConfiguration);
            var result = CastProducerResult<T>(obj);
            return result;
        }

        public static IConsumer<T> GetStaticConsumer<T>(this StaticPortMetadata portMetadata, PortConfiguration portConfiguration, object instance) {
            dynamic obj = GetStaticConnector(instance, portMetadata, portConfiguration);
            return (IConsumer<T>)obj;
        }

        /// <summary>
        /// Requirement: all local input metadata should be StaticPortMetadata, and all remote output connectors should be IProducer<T>
        /// </summary>
        /// <param name="componentConfiguration"></param>
        /// <param name="instance"></param>
        /// <param name="instantiatedComponents"></param>
        public static void ConnectAllStaticInputs(this ComponentConfiguration componentConfiguration, object instance, IReadOnlyList<ComponentEnvironment> instantiatedComponents) {
            foreach (var inputConfig in componentConfiguration.Inputs) {
                var inputMetadata = componentConfiguration.FindPortMetadata(inputConfig.LocalPort);
                Debug.Assert(inputMetadata.Direction == PortDirection.Input);
                var inputStaticMetadata = inputMetadata as StaticPortMetadata;
                if (inputStaticMetadata is null) {
                    continue;
                }
                var dataType = inputStaticMetadata.DataType;
                var getConsumerFunc = typeof(HelperExtensions).GetMethod(nameof(GetStaticConsumer)).MakeGenericMethod(dataType);
                dynamic consumer = getConsumerFunc.Invoke(null, new object[] { inputStaticMetadata, inputConfig.LocalPort, instance });

                var remoteEnvironment = instantiatedComponents.Single(e => inputConfig.RemoteId == e.Configuration.Id);
                var remoteOutputMetadata = remoteEnvironment.FindPortMetadata(inputConfig.RemotePort);
                Debug.Assert(remoteOutputMetadata.Direction == PortDirection.Output);
                var getProducerFunc = typeof(HelperExtensions).GetMethod(nameof(GetProducer)).MakeGenericMethod(dataType);
                dynamic producer = getProducerFunc.Invoke(null, new object[] { remoteEnvironment, inputConfig.RemotePort});

                Operators.PipeTo(producer, consumer, inputConfig.DeliveryPolicy);
            }
        }

        public static bool IsAssignableToGenericType(this Type givenType, Type genericType) {
            if (!genericType.IsGenericTypeDefinition) {
                throw new ArgumentException("Type should be an open generic type.", nameof(genericType));
            }
            return IsAssignableToGenericType_Internal(givenType, genericType);
        }

        private static bool IsAssignableToGenericType_Internal(Type givenType, Type genericType) {
            var interfaceTypes = givenType.GetInterfaces();
            foreach (var it in interfaceTypes) {
                if (it.IsGenericType && it.GetGenericTypeDefinition() == genericType) {
                    return true;
                }
            }
            if (givenType.IsGenericType && givenType.GetGenericTypeDefinition() == genericType) {
                return true;
            }
            var baseType = givenType.BaseType;
            if (baseType == null) {
                return false;
            }
            return IsAssignableToGenericType_Internal(baseType, genericType);
        }

        private static bool IsNullable(Type type) {
            var result = type.IsGenericType 
                && type.GetGenericTypeDefinition() == typeof(Nullable<>) 
                && !type.GetGenericArguments().Single().IsGenericType //We supports only one layer of nesting
                ;
            return result;
        }

        private static bool HasConversionTo_SingleWay(this Type source, Type target, bool unwrapNullables) {
            /** Nullable
             */
            if (unwrapNullables) {
                var sourceIsNullable = IsNullable(source);
                var targetIsNullable = IsNullable(target);
                switch (sourceIsNullable, targetIsNullable) {
                    case (true, false):
                        return false;
                    case (false, true):
                        return HasConversionTo_SingleWay(
                            source, 
                            target.GetGenericArguments()[0], 
                            unwrapNullables: true
                        );
                    case (true, true):
                        return HasConversionTo_SingleWay(
                            source.GetGenericArguments()[0],
                            target.GetGenericArguments()[0],
                            unwrapNullables: true
                        );
                } 
            }

            /** Built-in (Conversion done by compiler)
             */
            var bothConvertable = typeof(IConvertible).IsAssignableFrom(source) 
                && typeof(IConvertible).IsAssignableFrom(target);
            if (bothConvertable) {
                return true;
            }

            /** Custom
             */
            var result = GetImplicitOrExplicitConverters(source, target).Any();
            return result;
        }

        private static IEnumerable<MethodInfo> GetImplicitOrExplicitConverters_SingleWay(this Type type, Type source, Type target) {
            var result = type.GetMethods(BindingFlags.Public | BindingFlags.Static)
                .Where(mi => mi.Name == "op_Implicit" || mi.Name == "op_Explicit")
                .Where(mi => mi.ReturnType == target)
                .Where(mi => {
                    var parameterInfos = mi.GetParameters();
                    if (parameterInfos.Length != 1) {
                        return false;
                    } 
                    var result = parameterInfos.Single().ParameterType == source;
                    return result;
                });
            return result;
        }

        private static IEnumerable<MethodInfo> GetImplicitOrExplicitConverters(this Type source, Type target) {
            foreach (var mi in GetImplicitOrExplicitConverters_SingleWay(source, source, target)) {
                yield return mi;
            }
            foreach (var mi in GetImplicitOrExplicitConverters_SingleWay(target, source, target)) {
                yield return mi;
            }
        }

        public static bool CanBeCastTo(this Type sourceType, Type targetType) {
            var result = targetType.IsAssignableFrom(sourceType) 
                || HasConversionTo_SingleWay(sourceType, targetType, unwrapNullables: true) 
                || HasConversionTo_SingleWay(targetType, sourceType, unwrapNullables: false);
            return result;
        }

        private static Type GetProducerResultType(object instance) {
            var instanceType = instance.GetType();
            if (!instanceType.IsGenericType) {
                throw new InvalidOperationException("The instance is not a generic type.");
            }
            var interfaceName = typeof(IProducer<>).Name;//should be "IProducer`1"
            var @interface = instanceType.GetInterface(interfaceName);
            if (@interface is null) {
                throw new InvalidOperationException("The instance does not implement IProducer<>.");
            }
            var typeArg = @interface.GetGenericArguments().Single();
            return typeArg;
        }

        public static bool CanProducerResultBeCastTo<T>(object instance) {
            var argType = GetProducerResultType(instance);
            var result = argType.CanBeCastTo(typeof(T));
            return result;
        }

        public static IProducer<T> CastProducerResult<T>(object instance) {
            var instanceType = instance.GetType();
            if (typeof(IProducer<T>).IsAssignableFrom(instanceType)) {
                return (IProducer<T>)instance;
            }
            var argType = GetProducerResultType(instance);
            if (!argType.CanBeCastTo(typeof(T))) {
                throw new InvalidOperationException($"The return type of producer \"{argType.Name}\" cannot be converted to \"{typeof(T).Name}\".");
            }
            Debug.Assert(argType != typeof(T));//If match, then should have returned at the beginning.
            var methodInfo = typeof(HelperExtensions)
                .GetMethod(nameof(CastProducerResult_Internal), BindingFlags.NonPublic | BindingFlags.Static)
                .MakeGenericMethod(new[] {
                    argType,
                    typeof(T),
                });
            var result = (IProducer<T>)methodInfo.Invoke(obj: null, new[] { (dynamic)instance });
            return result;
        }

        private static IProducer<TTarget> CastProducerResult_Internal<TSource, TTarget>(this IProducer<TSource> producer) {
            if (typeof(TSource) == typeof(TTarget)) {
                return (IProducer<TTarget>)producer;
            }

            Debug.Assert(CanBeCastTo(typeof(TSource), typeof(TTarget)));

            var func = CastDelegate<TSource, TTarget>();

            return producer.Select(func);
        }

        private static Func<TSource, TTarget> CastDelegate<TSource, TTarget>() {
            if (typeof(TSource) == typeof(TTarget)) {
                throw new InvalidOperationException("TSource is the same as TTarget.");
            }

            var sourceIsNullable = IsNullable(typeof(TSource));
            var targetIsNullable = IsNullable(typeof(TTarget));
            switch (sourceIsNullable, targetIsNullable) {
                case (true, false):
                    throw new InvalidOperationException();
                case (false, false):
                    var bothConvertable = typeof(IConvertible).IsAssignableFrom(typeof(TSource))
                        && typeof(IConvertible).IsAssignableFrom(typeof(TTarget));
                    if (bothConvertable) {
                        if (typeof(TSource) == typeof(float) && typeof(TTarget) == typeof(double)) {
                            return v => (TTarget)(object)Convert.ToDouble((float)(object)v);
                        } else {
                            return CastConvertable<TSource, TTarget>;//Will box object
                        }
                    } else {
                        var converterMethods = GetImplicitOrExplicitConverters(typeof(TSource), typeof(TTarget)).ToArray();
                        if (converterMethods.Length > 0) {
                            Debug.Assert(converterMethods.Length == 1, "It is OK to have more then 1 matched converter methods here, but their return values should be the same, the following code only uses the first converter.");
                            var converterMethod = converterMethods.First();
                            var result = (Func<TSource, TTarget>)Delegate.CreateDelegate(typeof(Func<TSource, TTarget>), converterMethod);
                            return result;
                        } else {
                            //Fallback to cast, likly to fail
                            return Cast<TSource, TTarget>;
                        }
                    }
                case (true, true):
                    var nestedSourceType = typeof(TSource).GetGenericArguments()[0];
                    var nestedTargetType = typeof(TTarget).GetGenericArguments()[0];
                    var method1 = typeof(HelperExtensions)
                        .GetMethod(nameof(CastBetweenNullables), BindingFlags.NonPublic | BindingFlags.Static)
                        .MakeGenericMethod(new[] {
                            nestedSourceType,
                            nestedTargetType
                        });
                    var funcType = typeof(Func<,>).MakeGenericType(nestedSourceType, nestedTargetType);
                    return (Func<TSource, TTarget>)Delegate.CreateDelegate(funcType, method1);
                case (false, true):
                    var unwrappedTargetType = typeof(TTarget).GetGenericArguments()[0];
                    if (typeof(TSource) == unwrappedTargetType) {
                        var nullableImplicitConverterMethod = GetImplicitOrExplicitConverters_SingleWay(typeof(TTarget), typeof(TSource), typeof(TTarget)).Single();
                        var result = (Func<TSource, TTarget>)Delegate.CreateDelegate(typeof(Func<TSource, TTarget>), nullableImplicitConverterMethod);
                        return result;
                    } else {
                        dynamic method2 = typeof(HelperExtensions)
                            .GetMethod(nameof(CastDelegate), BindingFlags.NonPublic | BindingFlags.Static) //Nested nullable occasion
                            .MakeGenericMethod(new[] {
                                typeof(TSource),
                                unwrappedTargetType,
                            })
                            .Invoke(obj: null, parameters: Array.Empty<object>());
                        return v => (TTarget)method2(v);
                    }
                    
            }
        }

        private static TTarget Cast<TSource, TTarget>(TSource source) {
            var result = (TTarget)(object)source;
            return result;
        }

        private static TTarget CastConvertable<TSource, TTarget>(TSource source){
            var result = (TTarget)Convert.ChangeType(source, typeof(TTarget));//object boxed
            return result;
        }

        private static TTarget? CastBetweenNullables<TSource, TTarget>(TSource? source) 
            where TSource : struct 
            where TTarget : struct {
            if (!source.HasValue) {
                return null;
            }
            var func = CastDelegate<TSource, TTarget>();
            TTarget? result = func(source.Value);
            return result;
        }

        public static object DefaultPortIndex(this PortAggregation aggregation) {
            switch (aggregation) {
                case PortAggregation.Object:
                    return null;
                case PortAggregation.List:
                    return 0.ToString();
                case PortAggregation.Dictionary:
                    return string.Empty;
                default:
                    throw new InvalidOperationException();
            }
        }

        #region data type finder
        public static Type FindInputPortDataType(this ComponentConfiguration config, IPortMetadata portMetadata, IReadOnlyList<ComponentConfiguration> configs, params Tuple<ComponentConfiguration, IPortMetadata>[] exclude) {
            var dataType = portMetadata.GetTransmissionDataType(null, Array.Empty<Type>(), Array.Empty<Type>());
            if (dataType is null) {
                var i = config.Inputs.FirstOrDefault(c => Equals(c.LocalPort.Identifier, portMetadata.Identifier));
                if (i is null) {
                    return null;
                }
                foreach (var other in configs) {
                    if (Equals(i.RemoteId, other.Id)) {
                        var newExclude = new Tuple<ComponentConfiguration, IPortMetadata>[exclude.Length + 1];
                        Array.Copy(exclude, newExclude, exclude.Length);
                        var oMetadata = other.FindPortMetadata(i.RemotePort);
                        newExclude[exclude.Length] = new Tuple<ComponentConfiguration, IPortMetadata>(other, oMetadata);
                        var otherInput = FindInputPortDataTypes(other, configs, newExclude);
                        var otherOutput = FindOutputPortDataTypes(other, configs, newExclude);
                        var otherEnd = oMetadata.GetTransmissionDataType(null, otherInput, otherOutput);
                        if (otherEnd != null) {
                            dataType = portMetadata.GetTransmissionDataType(otherEnd, Array.Empty<Type>(), Array.Empty<Type>());
                            if (dataType != null) {
                                goto jump;
                            }
                            newExclude[exclude.Length] = new Tuple<ComponentConfiguration, IPortMetadata>(config, portMetadata);
                            var selfOutput = FindOutputPortDataTypes(config, configs, newExclude);
                            dataType = portMetadata.GetTransmissionDataType(otherEnd, selfOutput, Array.Empty<Type>());
                            if (dataType != null) {
                                goto jump;
                            }
                            var selfInput = FindInputPortDataTypes(config, configs, newExclude);
                            dataType = portMetadata.GetTransmissionDataType(otherEnd, selfOutput, selfInput);
                            if (dataType != null) {
                                goto jump;
                            }
                        }

                    }
                }
jump:;
            }
            return dataType;
        }

        public static IList<Type> FindInputPortDataTypes(this ComponentConfiguration config, IReadOnlyList<ComponentConfiguration> configs, params Tuple<ComponentConfiguration, IPortMetadata>[] exclude) {
            var inputPorts = config.GetMetadata().InputPorts().ToArray();
            var result = new Type[inputPorts.Length];
            var idx = 0;
            foreach (var iMetadata in inputPorts) {
                Type dataType;
                if (exclude != null && exclude.Any(ex => ex.Item1.Id == config.Id && Equals(ex.Item2.Identifier, iMetadata.Identifier))) {
                    dataType = null;
                } else {
                    dataType = FindInputPortDataType(config, iMetadata, configs, exclude);
                }
                result[idx] = dataType;
                idx++;
            }
            return result;
        }

        public static IList<Type> FindInputPortDataTypes(this ComponentConfiguration config, IReadOnlyList<ComponentConfiguration> configs, IPortMetadata exclude) {
            return FindInputPortDataTypes(config, configs, new Tuple<ComponentConfiguration, IPortMetadata>(config, exclude));
        }

        //TODO: make this method non-recursive. The current implementation may get Stack Overflow if the pipeline is long.
        public static Type FindOutputPortDataType(this ComponentConfiguration config, IPortMetadata portMetadata, IReadOnlyList<ComponentConfiguration> configs, params Tuple<ComponentConfiguration, IPortMetadata>[] exclude) {
            /** Try with no information given.
             */
            var dataType = portMetadata.GetTransmissionDataType(null, Array.Empty<Type>(), Array.Empty<Type>());

            /** Try with local input types given.
             */
            if (dataType is null) {
                var inputTypes = config.FindInputPortDataTypes(configs);
                dataType = portMetadata.GetTransmissionDataType(null, inputTypes, Array.Empty<Type>());
            }

            if (dataType is null) {
                foreach (var other in configs) {
                    foreach (var i in other.Inputs) {
                        var iMetadata = other.FindPortMetadata(i.LocalPort);
                        if (Equals(i.RemoteId, config.Id) && Equals(i.RemotePort.Identifier, portMetadata.Identifier)) {
                            var newExclude = new Tuple<ComponentConfiguration, IPortMetadata>[exclude.Length + 1];
                            Array.Copy(exclude, newExclude, exclude.Length);
                            newExclude[exclude.Length] = new Tuple<ComponentConfiguration, IPortMetadata>(other, iMetadata);
                            var otherOutput = FindOutputPortDataTypes(other, configs, newExclude);
                            var otherInput = FindInputPortDataTypes(other, configs, newExclude);
                            var otherEnd = iMetadata.GetTransmissionDataType(null, otherOutput, otherInput);
                            if (otherEnd != null) {
                                /** Try with only remote input type given
                                 */
                                dataType = portMetadata.GetTransmissionDataType(otherEnd, Array.Empty<Type>(), Array.Empty<Type>());
                                if (dataType != null) {
                                    goto jump;
                                }

                                /** Try with remote input type and local input types given
                                 */
                                newExclude[exclude.Length] = new Tuple<ComponentConfiguration, IPortMetadata>(config, portMetadata);
                                var selfInput = FindInputPortDataTypes(config, configs, newExclude);
                                dataType = portMetadata.GetTransmissionDataType(otherEnd, selfInput, Array.Empty<Type>());
                                if (dataType != null) {
                                    goto jump;
                                }

                                /** Try with remote input type, local input types and local output types given
                                 */
                                var selfOutput = FindOutputPortDataTypes(config, configs, newExclude);
                                dataType = portMetadata.GetTransmissionDataType(otherEnd, selfInput, selfOutput);
                                if (dataType != null) {
                                    goto jump;
                                }
                            }
                        }
                    }
                }
jump:;
            }
            return dataType;
        }

        public static IList<Type> FindOutputPortDataTypes(this ComponentConfiguration config, IReadOnlyList<ComponentConfiguration> configs, params Tuple<ComponentConfiguration, IPortMetadata>[] exclude) {
            var result = config
                .GetMetadata()
                .OutputPorts()
                .Select(oMetadata => {
                    var ret = FindOutputPortDataType(config, oMetadata, configs, exclude);
                    return ret;
                })
                .ToArray();
            return result;
        }

        public static IList<Type> FindOutputPortDataTypes(this ComponentConfiguration config, IReadOnlyList<ComponentConfiguration> configs, IPortMetadata exclude) {
            return FindOutputPortDataTypes(config, configs, new Tuple<ComponentConfiguration, IPortMetadata>(config, exclude));
        }
        #endregion

        public static IEnumerable<Assembly> LoadAssemblies(string rootPath) {
            var files = Directory.EnumerateFiles(AppDomain.CurrentDomain.BaseDirectory, "*.dll");
            foreach (var file in files) {
                /** Test if it is a valid .NET assembly without throwing any exception.
                 */
                using (var fs = new FileStream(file, FileMode.Open, FileAccess.Read, FileShare.ReadWrite)) {
                    using var peReader = new PEReader(fs);
                    if (!peReader.HasMetadata) {
                        continue;
                    }
                    var reader = peReader.GetMetadataReader();
                    if (!reader.IsAssembly) {
                        continue;
                    }
                }

                /** Try to load assembly.
                  */
                Assembly asm = null;
                try {
                    asm = Assembly.LoadFrom(file);
                } catch (BadImageFormatException) {
                    ;
                }
                if (asm is null) {
                    continue;
                }
                yield return asm;
            }
        }
    }
}
