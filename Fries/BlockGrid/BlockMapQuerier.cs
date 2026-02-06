using System;
using System.Collections.Generic;
using Fries.Data;
using Fries.Pool;
using UnityEngine;

namespace Fries.BlockGrid {
    public class BlockMapQuerier {
        internal const bool AT = true;
        internal const bool RANGE = false;
        internal const int LoopRange = 1;
        internal const int LoopDict = 2;

        private EverythingPool everythingPool;
        public BlockMapQuerier(EverythingPool everythingPool) {
            this.everythingPool = everythingPool;
        }

        # region 位置检索参数
        public void SetPositionRange(Vector3Int from, Vector3Int to) {
            this.at = from;
            this.to = to;
            usingAtOrRangePos = RANGE;
        }
        public void SetPositionAt(Vector3Int at) {
            this.at = at;
            usingAtOrRangePos = AT;
        }
        internal bool? usingAtOrRangePos;
        internal Vector3Int? at;
        internal Vector3Int? to;

        private void resetPositionParameters() {
            at = null;
            to = null;
            usingAtOrRangePos = null;
        }

        private QueryPositionEnumerable forPositions(BlockMap blockMap) {
            if (!blockMap) return default;
            return new QueryPositionEnumerable(this, blockMap.blockMap);
        }
        # endregion
        
        # region 方块类型检索参数

        public void SetBlockTypeRange(int from, int to) {
            blkTypeAt = from;
            blkTypeTo = to;
            usingAtOrRangeBlkType = RANGE;
        }
        public void SetBlockTypeAt(int at) {
            blkTypeAt = at;
            usingAtOrRangeBlkType = AT;
        }
        internal int? blkTypeAt = null;
        internal int? blkTypeTo = null;
        internal bool? usingAtOrRangeBlkType = null;

        private void resetBlockTypeParameters() {
            blkTypeAt = null;
            blkTypeTo = null;
            usingAtOrRangeBlkType = null;
        }
        
        private BlkIdEnumerable forBlkId(Dictionary<int, ListSet<Facing>> blkId2Facings) => new(this, blkId2Facings);
        # endregion
        
        # region 方块朝向检索参数
        public void SetFacing(Facing facing) {
            this.facing = facing;
            usingFacingSet = AT;
        }
        public void AddFacing(Facing facing) {
            facingSet.Add(facing);
            usingFacingSet = RANGE;
        }
        internal Facing? facing = null;
        internal readonly ListSet<Facing> facingSet = new();
        internal bool? usingFacingSet = null;
        private void resetFacingParameters() {
            facing = null;
            facingSet.Clear();
            usingFacingSet = null;
        }
        
        private FacingEnumerable forFacing(ListSet<Facing> facings) => new(this, facings);
        # endregion
        
        # region 顶部参数
        private bool isLookingForTopResult = false;
        public void SetLookingForTopResult(bool isLookingForTopResult) =>
            this.isLookingForTopResult = isLookingForTopResult;
        private void resetTopArguments() => isLookingForTopResult = false;

        private bool isTopGrid(Dictionary<Vector2Int, HashSet<BlockKey>> pos2Keys, Vector3Int gridPos, out bool shouldCreate, out bool shouldClear) {
            shouldClear = false;
            shouldCreate = false;
            if (!pos2Keys.TryGetValue(new Vector2Int(gridPos.x, gridPos.z), out var keys)) {
                shouldCreate = true;
                return true;
            }
            
            if (keys.Count == 0) return true;
            
            BlockKey? any = null;
            foreach (var key in keys) {
                any = key;
                break;
            }
            
            Debug.Assert(any.HasValue);
            if (any.Value.Position.y < gridPos.y) {
                shouldClear = true;
                return true;
            }
            if (any.Value.Position.y == gridPos.y) return true;
            return false;
        }
        # endregion
        
        private BlockMap blockMap;
        public void ResetParameters() {
            resetPositionParameters();
            resetBlockTypeParameters();
            resetFacingParameters();
            resetTopArguments();
            blockMap = null;
        }
        
        public bool Query(BlockMap blockMap = null, ISet<BlockKey> outBlockSet = null) {
            if (blockMap) this.blockMap = blockMap;
            if (!this.blockMap) throw new ArgumentException("BlockMap must be set in order to query!");

            Dictionary<Vector2Int, HashSet<BlockKey>> pos2Keys =
                everythingPool.ActivateObject<Dictionary<Vector2Int, HashSet<BlockKey>>>();
            
            try {
                foreach (var kvp0 in forPositions(this.blockMap)) {
                    var pos = kvp0.Key;
                    var blkId2Facings = kvp0.Value;
                    
                    bool isTop = false;
                    bool shouldClear = false;
                    bool shouldCreate = false;
                    if (isLookingForTopResult) isTop = isTopGrid(pos2Keys, pos, out shouldCreate, out shouldClear);
                    var v2d = new Vector2Int(pos.x, pos.z);
                    HashSet<BlockKey> keys = null;

                    foreach (var kvp in forBlkId(blkId2Facings)) {
                        // 如果不收集顶部方块
                        if (!isLookingForTopResult) {
                            foreach (var facing1 in forFacing(kvp.Value)) {
                                if (outBlockSet == null) return true;
                                outBlockSet.Add(new BlockKey(kvp.Key, pos, facing1));
                            }
                        }
                        // 如果只收集顶部方块
                        else {
                            if (!isTop) continue;
                            foreach (var facing1 in forFacing(kvp.Value)) {
                                if (outBlockSet == null) return true;
                                if (shouldCreate) {
                                    keys = everythingPool.ActivateObject<HashSet<BlockKey>>();
                                    pos2Keys[v2d] = keys;
                                    shouldCreate = false;
                                }
                                keys ??= pos2Keys[v2d];
                                
                                if (shouldClear) {
                                    shouldClear = false;
                                    keys.Clear();
                                }
                                keys.Add(new BlockKey(kvp.Key, pos, facing1));
                            }
                        }
                    }
                }

                if (isLookingForTopResult) {
                    foreach (var kv in pos2Keys) {
                        if (kv.Value.Count == 0) continue;
                        if (outBlockSet == null) return true;
                        outBlockSet.UnionWith(kv.Value);
                    }
                }
            }
            finally {
                foreach (var kv in pos2Keys)
                    everythingPool.DeactivateObject(kv.Value);
                everythingPool.DeactivateObject(pos2Keys);
            }
            
            if (outBlockSet != null && outBlockSet.Count != 0)
                return true;
            return false;
        }

        public void Remove(BlockMap blockMap = null, ISet<BlockKey> outFacingSet = null) {
            ISet<BlockKey> res;
            if (outFacingSet == null) res = everythingPool.ActivateObject<HashSet<BlockKey>>();
            else res = outFacingSet;
            
            try {
                Query(blockMap, res);
                this.blockMap.RemoveBlocks(res, null);
            }
            finally {
                everythingPool.DeactivateObject(res);
            }
        }
    }
}