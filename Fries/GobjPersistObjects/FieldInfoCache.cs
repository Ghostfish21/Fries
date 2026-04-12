using System;
using System.Collections.Generic;
using System.Reflection;

namespace Fries.GobjPersistObjects {
    public static class FieldInfoCache {
        private static readonly Dictionary<Type, Dictionary<string, FieldInfo>> _cache = new();

        private const BindingFlags BF = BindingFlags.Instance | BindingFlags.Public | 
                                        BindingFlags.NonPublic | BindingFlags.DeclaredOnly;

        private static readonly FieldInfo _miss = typeof(FieldInfoCache).GetField(nameof(_miss), 
            BindingFlags.Static | BindingFlags.NonPublic);

        internal static FieldInfo get(Type type, string fieldName) {
            if (type == null) throw new ArgumentNullException(nameof(type));
            if (fieldName == null) throw new ArgumentNullException(nameof(fieldName));

            if (!_cache.TryGetValue(type, out var perType)) {
                perType = new Dictionary<string, FieldInfo>(16, StringComparer.Ordinal);
                _cache[type] = perType;
            }

            if (perType.TryGetValue(fieldName, out var cached)) 
                return ReferenceEquals(cached, _miss) ? null : cached;

            var fi = type.GetField(fieldName, BF);
            perType[fieldName] = fi ?? _miss;
            return fi;
        }
    }
}