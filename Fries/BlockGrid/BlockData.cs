using System;
using System.Collections.Generic;
using System.Reflection;

namespace Fries.BlockGrid {
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
    public class BlockData : Attribute {
        public readonly int directionalType;

        public BlockData(DirectionalType directionalType) {
            this.directionalType = (int)directionalType;
        }
        public BlockData(int directionalType) {
            this.directionalType = directionalType;
        }
        
        private static Dictionary<int, BlockData> blockId2BlockData = new();
        public static BlockData GetBlockData<T>(T blockType) where T : Enum {
            return GetBlockData(blockType);
        }
        
        internal static BlockData GetBlockData(object blockType) {
            int blockTypeId = Convert.ToInt32(blockType);
            if (blockId2BlockData.TryGetValue(blockTypeId, out var blockData)) return blockData;
            FieldInfo blockDataField = blockType.GetType().GetField(blockType.ToString());
            BlockData data = (BlockData)blockDataField.GetCustomAttribute(typeof(BlockData));
            data ??= new BlockData(DirectionalType.NA);
            blockId2BlockData.Add(blockTypeId, data);
            return data;
        }
    }
}