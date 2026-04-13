using System;
using System.Collections.Generic;
using System.Globalization;
using Fries.EvtSystem;
using UnityEngine;

namespace Fries.GobjPersistObjects.Csv.Serialize {
    public static class SerializeHelper {
        // TODO:
        // (Object) List<T>, string typeName => string value
        // Object, string typeName => string value
        // ==============================================
        // T value => (Object) List<T>, string typeName
        // T value => Object, string typeName

        private static bool isIniting = false;
        private static bool isReady = false;
        private static HashSet<Type> parsableTypes;
        private static Dictionary<Type, Func<string>> typeNameGetters;
        private static Dictionary<Type, Func<object, string>> serializers;
        private static Dictionary<Type, Func<string, object>> deserializers;
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void reset() {
            parsableTypes = null;
            typeNameGetters = null;
            serializers = null;
            deserializers = null;
            isReady = false;
            isIniting = false;
        }

        [EvtDeclarer] public struct OnCsvSaveFormatLoad { }
        [EvtListener(typeof(Events.OnEvtsysLoaded))]
        private static void loadSystem() {
            parsableTypes = new HashSet<Type>();
            typeNameGetters = new Dictionary<Type, Func<string>>();
            serializers = new Dictionary<Type, Func<object, string>>();
            deserializers = new Dictionary<Type, Func<string, object>>();
            isIniting = true;
            Evt.TriggerNonAlloc<OnCsvSaveFormatLoad>();
            isIniting = false;
            isReady = true;
        }

        private static void guaranteeCanConfig() {
            if (!isIniting) throw new InvalidOperationException("This method can only be run during OnCsvSaveFormatLoad!");
        }
        private static void guaranteeNoDuplicate(string typeName) {
            foreach (var parsableType in parsableTypes) {
                if (typeNameGetters[parsableType]() == typeName) throw new InvalidOperationException($"Typename {typeName} already exists!");
            }
        }
        public static void AddParsableType(Type type, Func<string> typeNameGetter, 
            Func<object, string> serializer, Func<string, object> deserializer) {
            guaranteeCanConfig();
            guaranteeNoDuplicate(typeNameGetter());
            
            parsableTypes.Add(type);
            typeNameGetters.Add(type, typeNameGetter);
            serializers.Add(type, serializer);
            deserializers.Add(type, deserializer);
        }
        public static void AddParsableType(ISaveSerializable type) {
            guaranteeCanConfig();
            guaranteeNoDuplicate(type.GetTypeName());
            
            Type t = type.GetType();
            parsableTypes.Add(t);
            typeNameGetters.Add(t, type.GetTypeName);
            serializers.Add(t, obj => ((ISaveSerializable)obj).Serialize());
            deserializers.Add(t, type.Deserialize);
        }
        [EvtListener(typeof(OnCsvSaveFormatLoad))]
        private static void loadTypes() {
            AddParsableType(typeof(string), () => "string",
                obj => (string)obj, raw => raw);
            AddParsableType(typeof(Vector2), () => "Vector2", 
                obj => Utility.ParseVec2((Vector2)obj), raw => Utility.ParseVec2(raw));
            AddParsableType(typeof(Vector3), () => "Vector3",
                obj => Utility.ParseVec3((Vector3)obj), raw => Utility.ParseVec3(raw));
            
            AddParsableType(typeof(byte), () => "byte",
                obj => ((IFormattable)obj).ToString(null, CultureInfo.InvariantCulture),
                raw => byte.Parse(raw, CultureInfo.InvariantCulture));
            AddParsableType(typeof(short), () => "short",
                obj => ((IFormattable)obj).ToString(null, CultureInfo.InvariantCulture),
                raw => short.Parse(raw, CultureInfo.InvariantCulture));
            AddParsableType(typeof(int), () => "int",
                obj => ((IFormattable)obj).ToString(null, CultureInfo.InvariantCulture),
                raw => int.Parse(raw, CultureInfo.InvariantCulture));
            AddParsableType(typeof(long), () => "long",
                obj => ((IFormattable)obj).ToString(null, CultureInfo.InvariantCulture),
                raw => long.Parse(raw, CultureInfo.InvariantCulture));
            
            AddParsableType(typeof(float), () => "float",
                obj => ((float)obj).ToString("R", CultureInfo.InvariantCulture),
                raw => float.Parse(raw, CultureInfo.InvariantCulture));
            AddParsableType(typeof(double), () => "double",
                obj => ((double)obj).ToString("R", CultureInfo.InvariantCulture),
                raw => double.Parse(raw, CultureInfo.InvariantCulture));

            AddParsableType(typeof(bool), () => "bool",
                obj => ((bool)obj).ToString(),          // "True"/"False"
                raw => bool.Parse(raw));
            AddParsableType(typeof(char), () => "char",
                obj => ((char)obj).ToString(),
                raw => {
                    if (string.IsNullOrEmpty(raw)) return '\0';
                    if (raw.Length != 1) 
                        Debug.LogWarning("Input raw char's length must be 1!");
                    return raw[0];
                });
            AddParsableType(typeof(Quaternion), () => "Quaternion",
                obj => Utility.ParseQuat((Quaternion)obj),
                raw => Utility.ParseQuat(raw));
        }
        
        private static void guaranteeReadied() {
            if (!isReady) throw new InvalidOperationException("Please only run this method after OnCsvSaveFormatLoad event is over!");
        }

        private static bool isGenericList(object value, out Type listElemType) {
            listElemType = null;
            Type t = value.GetType();
            if (t.IsGenericType && t.GetGenericTypeDefinition() == typeof(List<>)) {
                listElemType = t.GetGenericArguments()[0];
                return true;
            }
            return false;
        }

        private static bool typeWalker(object value, out Type matchedChildmostType) {
            Type childestType = value.GetType();
            return typeWalker(childestType, out matchedChildmostType);
        }
        
        private static bool typeWalker(Type valueType, out Type matchedChildmostType) {
            matchedChildmostType = null;
            
            Debug.Assert(!valueType.IsGenericType);

            Type curType = valueType;
            if (parsableTypes.Contains(curType)) {
                matchedChildmostType = curType;
                return true;
            }
            
            while (curType.BaseType != null) {
                curType = curType.BaseType;
                if (!parsableTypes.Contains(curType)) continue;
                matchedChildmostType = curType;
                return true;
            }
            
            return false;
        }

        private static bool isParsable(Type listElemType, out Type type) => typeWalker(listElemType, out type);
        private static bool isParsable(string typeName, out Type type) {
            type = null;
            
            foreach (var parsableType in parsableTypes) {
                string typeName1 = typeNameGetters[parsableType]();
                if (typeName1 != typeName) continue;
                type = parsableType;
                return true;
            }

            return false;
        }
        public static bool IsParsable(object value, out Type type) {
            guaranteeReadied();
            if (isGenericList(value, out Type elemType)) 
                return isParsable(elemType, out type);
            return typeWalker(value, out type);
        }

        public static bool IsParsable4TName(string typeName, out Type type) {
            guaranteeReadied();
            string tName = typeName;
            if (tName.EndsWith("[]"))
                tName = tName.Substring(0, tName.Length - 2);

            return isParsable(tName, out type);
        }

        private static void guaranteeParsable(object value, out Type type) {
            if (!IsParsable(value, out type)) 
                throw new InvalidCastException(value + " is not a parsable type!");
        }
        private static void guaranteeParsable4TName(string typeName, out Type type) {
            if (!IsParsable4TName(typeName, out type)) 
                throw new InvalidCastException(typeName + " is not a parsable type!");
        }

        public static string GetTypeName(object value) {
            guaranteeParsable(value, out Type type);
            string rawTypeName = typeNameGetters[type]();
            if (isGenericList(value, out _)) rawTypeName += "[]";
            return rawTypeName;
        }

        public static string Serialize(object value) {
            guaranteeParsable(value, out Type type);
            if (isGenericList(value, out _)) 
                return ListSerializer.SerializeList(value, serializers[type]);
            return serializers[type](value);
        }

        public static object Deserialize(string typeName, string value) {
            guaranteeParsable4TName(typeName, out Type type);
            if (typeName.EndsWith("[]")) 
                return ListDeserializer.DeserializeList(value, type, deserializers[type]);
            return deserializers[type](value);
        }
    }
}