using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace Fries {
    public static class SystemUtils {
        public static long currentTimeMillis() {
            return DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond;
        }

        public static string projectName() {
            string dataPath = Application.dataPath;
            string projectName = new DirectoryInfo(dataPath).Parent.Name;
            return projectName;
        }
        
        public static bool hasAnnotation(this FieldInfo field, Type type) {
            if (field == null || type == null) return false;
            return Attribute.IsDefined(field, type, false);
        }

        public static T[] concat<T>(T[] first, T[] second) {
            var joined = new T[first.Length + second.Length];
            first.CopyTo(joined, 0);
            second.CopyTo(joined, first.Length);
            return joined;
        }

        private static Dictionary<string, long> _cache = new();

        public static long propertyToId(string name) {
            if (name is null) throw new ArgumentNullException(nameof(name));

            if (_cache.TryGetValue(name, out var id)) 
                return id;

            long computed = fnv1a64_Utf16(name);
            _cache[name] = computed;
            return computed;
        }

        private static long fnv1a64_Utf16(string s) {
            const ulong offset = 14695981039346656037UL;
            const ulong prime = 1099511628211UL;

            ulong hash = offset;

            unchecked {
                foreach (ushort ch in s) {
                    hash ^= (byte)(ch & 0xFF);
                    hash *= prime;

                    hash ^= (byte)(ch >> 8);
                    hash *= prime;
                }
            }
            return unchecked((long)hash);
        }
    }
}