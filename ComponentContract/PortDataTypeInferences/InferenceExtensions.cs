using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;

namespace OpenSense.Components.PortDataTypeInferences {

    internal static class InferenceExtensions {

        public static Type FindInputPortDataType(this ComponentConfiguration config, IPortMetadata port, IReadOnlyList<ComponentConfiguration> configs) {
            var exclusion = new InferenceExclusionItem(config, port);
            var exclusions = ImmutableList.Create(exclusion);
            var operation = new InferenceInputPort(config, port, configs, exclusions);
            RunInference(operation);
            var result = operation.GetResult();
            return result;
        }

        public static IList<Type> FindInputPortDataTypes(this ComponentConfiguration config, IReadOnlyList<ComponentConfiguration> configs, IPortMetadata excludePort) {
            var exclusion = new InferenceExclusionItem(config, excludePort);
            var exclusions = ImmutableList.Create(exclusion);
            var operation = new InferenceInputPorts(config, configs, exclusions);
            RunInference(operation);
            var result = operation.GetResult();
            return result;
        }

        public static Type FindOutputPortDataType(this ComponentConfiguration config, IPortMetadata port, IReadOnlyList<ComponentConfiguration> configs) {
            var exclusion = new InferenceExclusionItem(config, port);
            var exclusions = ImmutableList.Create(exclusion);
            var operation = new InferenceOutputPort(config, port, configs, exclusions);
            RunInference(operation);
            var result = operation.GetResult();
            return result;
        }

        public static IList<Type> FindOutputPortDataTypes(this ComponentConfiguration config, IReadOnlyList<ComponentConfiguration> configs, IPortMetadata excludePort) {
            var exclusion = new InferenceExclusionItem(config, excludePort);
            var exclusions = ImmutableList.Create(exclusion);
            var operation = new InferenceOutputPorts(config, configs, exclusions);
            RunInference(operation);
            var result = operation.GetResult();
            return result;
        }

        /// <summary>
        /// Simulate a stack to avoid StackOverflowException.
        /// </summary>
        private static void RunInference(InferenceOperation initialOperation) {
            var stack = new Stack<InferenceStackItem>();
            void pushStackCallback(InferenceOperation operation) {
                var item = new InferenceStackItem(operation);
                stack.Push(item);
            }

            pushStackCallback(initialOperation);

            try {
                while (stack.Count > 0) {
                    Debug.Assert(stack.Count < 1_000_000, "virtual stack too large, might indicate a bug.");
                    var top = stack.Peek();

                    /** Initialize
                     */
                    top.Progress ??= top.Operation.Run(pushStackCallback);

                    /** Progress
                     */
                    var isCompleted = !top.Progress.MoveNext();

                    /** Return
                     */
                    if (isCompleted) {
                        stack.Pop();
                    }
                }
            } catch (Exception ex) {
                ex.Data["OpenSense port data type inference virtual stack"] = stack;
                throw;
            }
        }

    }
}
