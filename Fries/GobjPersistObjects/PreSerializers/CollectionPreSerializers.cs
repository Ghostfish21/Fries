using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using Fries.EvtSystem;

namespace Fries.GobjPersistObjects.PreSerializers {
    public static class CollectionPreSerializers {
        private static object HashSetPS(object hashSet) {
            if (hashSet == null) throw new ArgumentNullException(nameof(hashSet));
            Type setType = hashSet.GetType();
            if (!setType.IsGenericType || setType.GetGenericTypeDefinition() != typeof(HashSet<>)) throw new ArgumentException("Argument must be HashSet<T>", nameof(hashSet));
            Type elementType = setType.GetGenericArguments()[0];
            var elemPS = PreSerializeHelper.GetSerializer(elementType);
            if (elemPS == null) throw new ArgumentException("No serializer found for element type " + elementType, nameof(hashSet));
            
            List<object> list = new();
            foreach (var item in (IEnumerable)hashSet) 
                list.Add(elemPS(item));
            
            return list;
        }
        // TODO 将这个实现改为更高效的反射
        private static object HashSetPDS(Type targetType, object listRepresentedHashSet) {
            if (targetType == null) throw new ArgumentNullException(nameof(targetType));
            if (listRepresentedHashSet == null) throw new ArgumentNullException(nameof(listRepresentedHashSet));
            if (!targetType.IsGenericType || targetType.GetGenericTypeDefinition() != typeof(HashSet<>)) throw new ArgumentException("targetType must be HashSet<T>", nameof(targetType));
            Type elementType = targetType.GetGenericArguments()[0];
            var elemPDS = PreSerializeHelper.GetDeserializer(elementType);
            if (elemPDS == null) throw new ArgumentException("No deserializer found for element type " + elementType, nameof(targetType));

            if (listRepresentedHashSet is not IEnumerable enumerable)
                throw new ArgumentException("Argument must be IEnumerable", nameof(listRepresentedHashSet));

            object hashSet = Activator.CreateInstance(targetType);
            MethodInfo addMethod = targetType.GetMethod("Add", new[] { elementType });

            foreach (var item in enumerable)
                addMethod.Invoke(hashSet, new[] { elemPDS(null, item) });

            return hashSet;
        }
        
        [EvtListener(typeof(OnGpoPreSerializerLoad))]
        private static void OnGpoPreSerializerLoad() {
            PreSerializeHelper.CreateSerializer(typeof(HashSet<>), HashSetPS);
            PreSerializeHelper.CreateDeserializer(typeof(HashSet<>), HashSetPDS);
        }
    }
}