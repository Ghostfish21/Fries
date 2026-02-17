using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.Analytics;

namespace Fries.BlockGrid.LevelEdit {
    public static class PartRegistry {
        private static Dictionary<int, (string, long)> partId2PrefabPath = new();
        private static Dictionary<int, (object, long)> id2Enum = new();

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void reset() {
            partId2PrefabPath ??= new Dictionary<int, (string, long)>();
            id2Enum ??= new Dictionary<int, (object, long)>();

            long sessionId = AnalyticsSessionInfo.sessionId;

            foreach (var partId in partId2PrefabPath.Keys.ToList()) {
                var value = partId2PrefabPath[partId];
                if (value.Item2 != sessionId) partId2PrefabPath.Remove(partId);
            }

            foreach (var partId in id2Enum.Keys.ToList()) {
                var value = id2Enum[partId];
                if (value.Item2 != sessionId) id2Enum.Remove(partId);
            }
        }

        public static void MapParts<T>(int startPartId, int endPartId, string path) where T : Enum {
            if (startPartId > endPartId) (startPartId, endPartId) = (endPartId, startPartId);
            for (int i = startPartId; i <= endPartId; i++) MapPart<T>(i, path);
        }

        public static void MapPart<T>(int partId, string path) where T : Enum {
            partId2PrefabPath[partId] = (path, AnalyticsSessionInfo.sessionId);
            if (!Enum.IsDefined(typeof(T), partId)) return;
            id2Enum[partId] = ((T)Enum.ToObject(typeof(T), partId), AnalyticsSessionInfo.sessionId);
        }

        private static string getPath(int partId) {
            if (!partId2PrefabPath.TryGetValue(partId, out var tuple))
                throw new KeyNotFoundException($"Cannot find path for part id {partId}!");
            if (tuple.Item2 != AnalyticsSessionInfo.sessionId)
                throw new KeyNotFoundException($"Cannot find path for part id {partId}!");
            return tuple.Item1;
        }

        public static string GetPath<T>(T partType, out int partId, StringBuilder builder = null, bool formated = true)
            where T : Enum {
            // 注意：这里必须调用 object overload，避免递归
            return GetPath((object)partType, out partId, builder, formated);
        }

        public static string GetPath(object partType, out int partId, StringBuilder builder = null, bool formated = true) {
            partId = Convert.ToInt32(partType);
            string path = getPath(partId);

            if (builder == null) {
                if (!path.EndsWith('/')) path += '/';
                string partName = partType.ToString().Replace('_', ' ');
                return path + partName;
            }

            builder.Append(path);
            if (!path.EndsWith('/')) builder.Append('/');

            string partName1 = partType.ToString();
            if (formated) {
                foreach (var curChar in partName1) {
                    if (curChar == '_') builder.Append(' ');
                    else builder.Append(curChar);
                }
            } else {
                builder.Append(partName1);
            }

            return builder.ToString();
        }

        public static object GetEnum(int id) {
            long sessionId = AnalyticsSessionInfo.sessionId;

            if (!id2Enum.TryGetValue(id, out var res)) return null;
            if (res.Item2 == sessionId) return res.Item1;
            return null;
        }
    }
}
