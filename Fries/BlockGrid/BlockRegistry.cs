using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.Analytics;

namespace Fries.BlockGrid {
    public static class BlockRegistry {
        private static Dictionary<int, (string, long)> blockId2PrefabPath = new();
        private static Dictionary<int, (object, long)> id2Enum = new();

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void reset() {
            blockId2PrefabPath ??= new Dictionary<int, (string, long)>();
            id2Enum ??= new Dictionary<int, (object, long)>();
            long sessionId = AnalyticsSessionInfo.sessionId;
            
            foreach (var blockId in blockId2PrefabPath.Keys.ToList()) {
                var value = blockId2PrefabPath[blockId];
                if (value.Item2 != sessionId) blockId2PrefabPath.Remove(blockId);
            }
            
            foreach (var blockId in id2Enum.Keys.ToList()) {
                var value = id2Enum[blockId];
                if (value.Item2 != sessionId) id2Enum.Remove(blockId);
            }
        }

        public static void MapBlocks<T>(int startBlockId, int endBlockId, string path) where T : Enum {
            if (startBlockId > endBlockId) (startBlockId, endBlockId) = (endBlockId, startBlockId);
            for (int i = startBlockId; i <= endBlockId; i++) MapBlock<T>(i, path);
        }

        public static void MapBlock<T>(int blockId, string path) where T : Enum {
            blockId2PrefabPath[blockId] = (path, AnalyticsSessionInfo.sessionId);
            if (!Enum.IsDefined(typeof(T), blockId)) return;
            id2Enum[blockId] = ((T)Enum.ToObject(typeof(T), blockId), AnalyticsSessionInfo.sessionId);
        }
        

        private static string getPath(int blockId) {
            if (!blockId2PrefabPath.TryGetValue(blockId, out var tuple))
                throw new KeyNotFoundException($"Cannot find path for block id {blockId}!");
            if (tuple.Item2 != AnalyticsSessionInfo.sessionId) 
                throw new KeyNotFoundException($"Cannot find path for block id {blockId}!");
            return tuple.Item1;
        }

        public static string GetPath<T>(T blockType, out int blockId, StringBuilder builder = null,
            bool formated = true) where T : Enum {
            return GetPath(blockType, out blockId, builder, formated);
        }

        public static string GetPath(object blockType, out int blockId, StringBuilder builder = null, bool formated = true) {
            blockId = Convert.ToInt32(blockType);
            string path = getPath(blockId);
            
            if (builder == null) {
                if (!path.EndsWith('/')) path += '/';
                string blockName = blockType.ToString().Replace('_', ' ');
                return path + blockName;
            }

            builder.Append(path);
            if (!path.EndsWith('/')) builder.Append('/');
            string blockName1 = blockType.ToString();
            if (formated) {
                foreach (var curChar in blockName1) {
                    if (curChar == '_') builder.Append(' ');
                    else builder.Append(curChar);
                }
            } else builder.Append(blockName1);

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