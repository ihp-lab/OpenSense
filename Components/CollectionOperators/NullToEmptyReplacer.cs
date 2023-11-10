#nullable enable

using System;
using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.Psi;
using Microsoft.Psi.Components;

namespace OpenSense.Components.CollectionOperators {
    public sealed class NullToEmptyReplacer<TElem, TCollection>
        : IConsumerProducer<TCollection?, TCollection>
        where TCollection : IEnumerable<TElem>{

        private static readonly TCollection? EmptyCollection;

        private Type? cachedType;

        private Type? usedTypeOfCachedType;

        private Lazy<TCollection?>? cachedEmptyCollection;

        public Receiver<TCollection?> In { get; }

        public Emitter<TCollection> Out { get; }

        static NullToEmptyReplacer() {
            var collectionType = typeof(TCollection);
            EmptyCollection = CreateEmptyCollection(collectionType);
        }

        public NullToEmptyReplacer(Pipeline pipeline) {
            In = pipeline.CreateReceiver<TCollection?>(this, Process, nameof(In));
            Out = pipeline.CreateEmitter<TCollection>(this, nameof(Out));
        }

        private void Process(TCollection? collection, Envelope envelope) {
            if (collection is not null) {
                UpdateCachedType(collection.GetType());
                Out.Post(collection, envelope.OriginatingTime);
                return;
            }
            if (EmptyCollection is not null) {
                Out.Post(EmptyCollection, envelope.OriginatingTime);
                return;
            }
            if (cachedEmptyCollection is not null) {
                /* a collection type is cached */
                var output = cachedEmptyCollection.Value;
                if (output is not null) {
                    /* the cached collection type is valid */
                    Out.Post(output, envelope.OriginatingTime);
                    return;
                }
            }
            throw new InvalidOperationException($"Cannot create an empty collection.");
        }

        private void UpdateCachedType(Type newType) {
            if (cachedType == newType) {
                return;
            }
            Type? newTypeToCache = null;
            if (cachedType is null) {
                newTypeToCache = newType;
            } else {
                //TODO: if the newType is better than the cached on, use it
            }
            if (newTypeToCache is not null) {
                cachedType = newTypeToCache;
                usedTypeOfCachedType = cachedType;//find a better type base on the input type and the TCollection
                cachedEmptyCollection = new Lazy<TCollection?>(CreateCachedEmptyCollection);
            }
        }

        private TCollection? CreateCachedEmptyCollection() {
            Debug.Assert(usedTypeOfCachedType is not null);
            var result = CreateEmptyCollection(usedTypeOfCachedType);
            return result;
        }

        private static TCollection? CreateEmptyCollection(Type collectionType) {
            if (collectionType.IsInterface) {
                if (collectionType.IsAssignableTo(typeof(TElem[]))) {
                    return (TCollection)(object)Array.Empty<TElem>();
                } else if (collectionType.IsAssignableTo(typeof(IList<TElem>))) {
                    return (TCollection)(object)new List<TElem>();
                } else if (collectionType.IsAssignableTo(typeof(IReadOnlyList<TElem>))) {
                    return (TCollection)(object)Array.Empty<TElem>();
                }
            } else {
                var constructor = collectionType.GetConstructor(Type.EmptyTypes);
                if (constructor is not null) {
                    return (TCollection)constructor.Invoke(Array.Empty<object>());
                }
            }
            return default;
        }
    }
}
