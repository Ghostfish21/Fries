using System;
using Fries.EvtSystem;
using UnityEngine;

namespace Fries.GobjPersistObjects.PreSerializers {
    public static class PrimitivePreSerializers {
        private static object PrimitivePS(object value) => value;
        private static object PrimitivePDS(Type _, object value) => value;
        
        [EvtListener(typeof(OnGpoPreSerializerLoad))]
        private static void OnGpoPreSerializerLoad(OnGpoPreSerializerLoad data) {
            PreSerializeHelper.CreateSerializer(typeof(short), PrimitivePS);
            PreSerializeHelper.CreateSerializer(typeof(int), PrimitivePS);
            PreSerializeHelper.CreateSerializer(typeof(long), PrimitivePS);
            PreSerializeHelper.CreateSerializer(typeof(bool), PrimitivePS);
            PreSerializeHelper.CreateSerializer(typeof(float), PrimitivePS);
            PreSerializeHelper.CreateSerializer(typeof(double), PrimitivePS);
            PreSerializeHelper.CreateSerializer(typeof(string), PrimitivePS);
            PreSerializeHelper.CreateSerializer(typeof(char), PrimitivePS);
            PreSerializeHelper.CreateSerializer(typeof(byte), PrimitivePS);
            
            PreSerializeHelper.CreateSerializer(typeof(Vector2), PrimitivePS);
            PreSerializeHelper.CreateSerializer(typeof(Vector3), PrimitivePS);
            PreSerializeHelper.CreateSerializer(typeof(Vector4), PrimitivePS);
            PreSerializeHelper.CreateSerializer(typeof(Quaternion), PrimitivePS);
            
            
            
            PreSerializeHelper.CreateDeserializer(typeof(short), PrimitivePDS );
            PreSerializeHelper.CreateDeserializer(typeof(int), PrimitivePDS );
            PreSerializeHelper.CreateDeserializer(typeof(long), PrimitivePDS );
            PreSerializeHelper.CreateDeserializer(typeof(bool), PrimitivePDS );
            PreSerializeHelper.CreateDeserializer(typeof(float), PrimitivePDS );
            PreSerializeHelper.CreateDeserializer(typeof(double), PrimitivePDS );
            PreSerializeHelper.CreateDeserializer(typeof(string), PrimitivePDS );
            PreSerializeHelper.CreateDeserializer(typeof(char), PrimitivePDS );
            PreSerializeHelper.CreateDeserializer(typeof(byte), PrimitivePDS );
            
            PreSerializeHelper.CreateDeserializer(typeof(Vector2), PrimitivePDS );
            PreSerializeHelper.CreateDeserializer(typeof(Vector3), PrimitivePDS );
            PreSerializeHelper.CreateDeserializer(typeof(Vector4), PrimitivePDS );
            PreSerializeHelper.CreateDeserializer(typeof(Quaternion), PrimitivePDS );
        }
    }
}