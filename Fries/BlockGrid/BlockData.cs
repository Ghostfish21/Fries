using System;
using System.Collections.Generic;
using System.Reflection;

namespace Fries.BlockGrid {
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
    public class BlockData : Attribute {
        public readonly DirectionalType DirectionalType;
        public BlockData(DirectionalType directionalType) {
            this.DirectionalType = directionalType;
        }
        
        private static Dictionary<int, BlockData> blockId2BlockData = new();
        public static BlockData GetBlockData<T>(T blockType) where T : Enum {
            int blockTypeId = Convert.ToInt32(blockType);
            if (blockId2BlockData.TryGetValue(blockTypeId, out var blockData)) return blockData;
            FieldInfo blockDataField = typeof(T).GetField(blockType.ToString());
            BlockData data = (BlockData)blockDataField.GetCustomAttribute(typeof(BlockData));
            data ??= new BlockData(DirectionalType.NA);
            blockId2BlockData.Add(blockTypeId, data);
            return data;
        }
    }
}