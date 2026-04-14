using System;
using System.Collections.Generic;
using Fries.EvtSystem;
using UnityEngine;

namespace Fries.GobjPersistObjects {
    [EvtDeclarer] public partial class OnGpoPreSerializerLoad { }

    public class PreSerializeHelper {
        private static Dictionary<Type, Func<object, object>> serializers = new();
        private static Dictionary<Type, Func<Type, object, object>> deserializers = new();
        
        [EvtListener(typeof(Events.OnEvtsysLoaded))]
        private static void OnEvtSysLoad() {
            serializers.Clear();
            deserializers.Clear();
            OnGpoPreSerializerLoad.TriggerNonAlloc();
        }
        
        public static void CreateSerializer(Type type, Func<object, object> serializer) =>
            serializers.TryAdd(type, serializer);
        public static Func<object, object> GetSerializer(Type type) =>
            serializers.GetValueOrDefault(type);
        public static Func<object, object> GetSerializer(object obj) =>
            serializers.GetValueOrDefault(obj.GetType());
        
        public static void CreateDeserializer(Type type, Func<Type, object, object> deserializer) =>
            deserializers.TryAdd(type, deserializer);
        public static Func<Type, object, object> GetDeserializer(Type type) =>
            deserializers.GetValueOrDefault(type);
        public static Func<Type, object, object> GetDeserializer(object obj) =>
            deserializers.GetValueOrDefault(obj.GetType());
    }
}