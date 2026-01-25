namespace Fries.BlockGrid {
    using System;
    using UnityEngine;

    public readonly struct BlockKey : IEquatable<BlockKey> {
        public readonly int BlockTypeId;
        public readonly Vector3Int Position;

        public BlockKey(int blockTypeId, Vector3Int position) {
            BlockTypeId = blockTypeId;
            Position = position;
        }

        public bool Equals(BlockKey other) =>
            BlockTypeId == other.BlockTypeId && Position.Equals(other.Position);

        public override bool Equals(object obj) =>
            obj is BlockKey other && Equals(other);

        public override int GetHashCode() {
            unchecked {
                // 简单稳定的组合 hash
                int hash = 17;
                hash = hash * 31 + BlockTypeId;
                hash = hash * 31 + Position.GetHashCode(); // Vector3Int 自己有 GetHashCode
                return hash;
            }
        }

        public static bool operator ==(BlockKey left, BlockKey right) => left.Equals(right);
        public static bool operator !=(BlockKey left, BlockKey right) => !left.Equals(right);

        public override string ToString() => $"({BlockTypeId}, {Position})";
    }

}