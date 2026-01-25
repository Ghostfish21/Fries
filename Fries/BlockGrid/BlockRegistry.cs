using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.Analytics;

namespace Fries.BlockGrid {
    public static class BlockRegistry {
        private static Dictionary<int, (string, long)> blockId2PrefabPath = new();

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void reset() {
            blockId2PrefabPath ??= new Dictionary<int, (string, long)>();
            long sessionId = AnalyticsSessionInfo.sessionId;
            foreach (var blockId in blockId2PrefabPath.Keys.ToList()) {
                var value = blockId2PrefabPath[blockId];
                if (value.Item2 != sessionId) blockId2PrefabPath.Remove(blockId);
            }
        }

        public static void MapBlocks(int startBlockId, int endBlockId, string path) {
            if (startBlockId > endBlockId) (startBlockId, endBlockId) = (endBlockId, startBlockId);
            for (int i = startBlockId; i <= endBlockId; i++) MapBlock(i, path);
        }
        public static void MapBlock(int blockId, string path) =>
            blockId2PrefabPath[blockId] = (path, AnalyticsSessionInfo.sessionId);

        private static string getPath(int blockId) {
            if (!blockId2PrefabPath.TryGetValue(blockId, out var tuple))
                throw new KeyNotFoundException($"Cannot find path for block id {blockId}!");
            if (tuple.Item2 != AnalyticsSessionInfo.sessionId) 
                throw new KeyNotFoundException($"Cannot find path for block id {blockId}!");
            return tuple.Item1;
        }
        public static string GetPath<T>(T blockType, out int blockId, StringBuilder builder = null, bool formated = true) where T : Enum {
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
                    builder.Append(curChar);
                }
            } else builder.Append(blockName1);

            return builder.ToString();
        }
    }
}