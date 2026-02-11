using Fries.Data;

namespace Fries.BlockGrid {
    using System;
    using UnityEngine;

    [Serializable]
    public struct BlockKey : IEquatable<BlockKey> {
        [SerializeField] public int BlockTypeId;
        [SerializeField] public Vector3Int Position;
        [SerializeField] public Facing Facing;

        public BlockKey(int blockTypeId, Vector3Int position, Facing facing) {
            BlockTypeId = blockTypeId;
            Position = position;
            Facing = facing;
        }

        public bool Equals(BlockKey other) =>
            BlockTypeId == other.BlockTypeId &&
            Position.Equals(other.Position) &&
            Facing == other.Facing;

        public override bool Equals(object obj) =>
            obj is BlockKey other && Equals(other);

        public override int GetHashCode() {
            unchecked {
                int hash = 17;
                hash = hash * 31 + BlockTypeId;
                hash = hash * 31 + Position.GetHashCode();
                hash = hash * 31 + (int)Facing;
                return hash;
            }
        }

        public static bool operator ==(BlockKey left, BlockKey right) => left.Equals(right);
        public static bool operator !=(BlockKey left, BlockKey right) => !left.Equals(right);

        public override string ToString() => $"({BlockTypeId}, {Position}, {Facing})";
    }
}