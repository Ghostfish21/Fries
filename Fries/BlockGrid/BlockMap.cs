using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Fries.BlockGrid.Fries.BlockGrid;
using Fries.Data;
using Fries.EvtSystem;
using Fries.Pool;
using UnityEngine;

namespace Fries.BlockGrid {
    public class BlockMap : MonoBehaviour {
        // TODO 制作 blockPool 的 Trim 机制，添加最多池元素上限
        // TODO HashSet<Facing> 没有池化，块量大时会产生很多小对象；如果墙体/面片很多，可以考虑用位掩码（4-bit）替代 HashSet，直接 byte facingMask。
        // TODO GetBlocksAtTop 每次遍历整个 blockMap（O(n)）；如果频繁调用，建议维护一个 (x,z)->topY 的索引缓存。
        // TODO partMap Bounds/Corner 的计算 默认 BlockMap transform 无旋转/无缩放
        
        private static readonly Dictionary<int, GameObject> prefabCache = new();

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void reset() => prefabCache.Clear();

        private static GameObject findPrefab(int id, string path) {
            if (prefabCache.TryGetValue(id, out var prefab)) return prefab;
            GameObject go = Resources.Load<GameObject>(path);
            if (!go) throw new ArgumentException("Given path does not exist (GameObject): " + path);
            prefabCache[id] = go;
            return go;
        }

        [EvtDeclarer]
        public struct OnBlockMapInit {
            BlockMap blockMap;
        }

        private EverythingPool _everythingPool;

        public EverythingPool everythingPool {
            private get => _everythingPool;
            set {
                if (_everythingPool) throw new InvalidOperationException("Cannot set EverythingPool twice!");
                partMap = new IrregularPartMap(UnitLength, value, transform);
                _everythingPool = value;
            }
        }

        public IrregularPartMap partMap { get; private set; }

        private void Awake() {
            Evt.TriggerNonAlloc<OnBlockMapInit>(this);
        }

        [SerializeField] private float unitLength = 1f;
        public float UnitLength => unitLength;
        private Dictionary<Vector3Int, Dictionary<int, HashSet<Facing>>> blockMap = new();
        private Dictionary<int, Dictionary<BlockKey, GameObject>> blockInstances = new();
        private Dictionary<int, Stack<GameObject>> blockPool = new();
        private Dictionary<BlockKey, int> blockBoundaryIds = new();
        private Dictionary<GameObject, BlockKey> instance2Key = new();
        private Dictionary<BlockKey, Dictionary<int, object>> blockData = new();

        public readonly List<(int, object)> CustomDataRegister = new();

        public void SetBlock<T>(Vector3Int at, T blkType, Facing direction = Facing.north, bool writeToPartMap = false)
            where T : Enum {
            SetBlock(at, at, blkType, direction, writeToPartMap);
        }

        public bool TryGetData(BlockKey key, out Dictionary<int, object> dataDict) =>
            blockData.TryGetValue(key, out dataDict);

        public const int FACING = 0;

        public void SetBlock<T>(Vector3Int pos1, Vector3Int pos2, T blockType, Facing direction = Facing.north,
            bool writeToPartMap = false) where T : Enum {
            if (!everythingPool)
                throw new ArgumentException("Must set EverythingPool before use by setting BlockMap.everythingPool");

            StringBuilder builder = everythingPool.ActivateObject<StringBuilder>();
            string prefabPath = BlockRegistry.GetPath(blockType, out int blockId, builder);
            everythingPool.DeactivateObject(builder);

            GameObject prefab = null;
            int xStart = pos1.x;
            int yStart = pos1.y;
            int zStart = pos1.z;
            int xEnd = pos2.x;
            int yEnd = pos2.y;
            int zEnd = pos2.z;
            if (xStart > xEnd) (xStart, xEnd) = (xEnd, xStart);
            if (yStart > yEnd) (yStart, yEnd) = (yEnd, yStart);
            if (zStart > zEnd) (zStart, zEnd) = (zEnd, zStart);
            for (int x = xStart; x <= xEnd; x++)
            for (int y = yStart; y <= yEnd; y++)
            for (int z = zStart; z <= zEnd; z++) {
                Vector3Int pos = new Vector3Int(x, y, z);
                BlockKey key = new BlockKey(blockId, pos, direction);

                if (GetBlocksOfType(blockType, pos, pos, direction, null)) RemoveBlocks(blockType, pos, pos, direction, null);

                if (writeToPartMap) {
                    int id = partMap.AddBounds(GetCellWorldPosBoundary(pos));
                    blockBoundaryIds[key] = id;
                }

                GameObject inst = null;
                if (!blockPool.TryGetValue(blockId, out Stack<GameObject> pool))
                    blockPool[blockId] = new Stack<GameObject>();
                else if (pool.Count != 0)
                    inst = pool.Pop();

                prefab ??= findPrefab(blockId, prefabPath);
                if (!prefab) throw new FileNotFoundException($"There is no prefab on path {prefabPath}!");
                if (!inst) inst = Instantiate(prefab);
                
                inst.transform.SetParent(transform, false);
                inst.transform.localScale = 1f.fff();
                inst.transform.localEulerAngles = 0f.fff();
                DirectioonalBlockApplier.apply(blockType, inst.transform, direction);
                inst.transform.localPosition = prefab.transform.localPosition +
                                               new Vector3(x * unitLength, y * unitLength, z * unitLength);
                inst.SetActive(true);

                if (!blockMap.TryGetValue(pos, out var blocks)) {
                    blocks = everythingPool.ActivateObject<Dictionary<int, HashSet<Facing>>>();
                    blockMap[pos] = blocks;
                }

                // TODO 改用对象池
                if (!blocks.TryGetValue(blockId, out var facings)) {
                    facings = new HashSet<Facing>();
                    blocks.Add(blockId, facings);
                }

                facings.Add(direction);

                // 在这里添加了 Block 的 CustomData 的编辑 {
                var dataDict = everythingPool.ActivateObject<Dictionary<int, object>>();
                blockData[key] = dataDict;
                dataDict[FACING] = direction;
                foreach (var tuple in CustomDataRegister)
                    dataDict[tuple.Item1] = tuple.Item2;
                // }

                if (!blockInstances.TryGetValue(blockId, out var dict)) {
                    dict = new Dictionary<BlockKey, GameObject>();
                    blockInstances[blockId] = dict;
                }

                dict[key] = inst;
                instance2Key[inst] = key;
            }
        }

        public void RemoveBlockBoundsFromPartMap(Vector3Int removeAt) =>
            RemoveBlockBoundsFromPartMap(removeAt, removeAt);

        public void RemoveBlockBoundsFromPartMap(Vector3Int pos1, Vector3Int pos2) {
            int xStart = pos1.x;
            int yStart = pos1.y;
            int zStart = pos1.z;
            int xEnd = pos2.x;
            int yEnd = pos2.y;
            int zEnd = pos2.z;
            if (xStart > xEnd) (xStart, xEnd) = (xEnd, xStart);
            if (yStart > yEnd) (yStart, yEnd) = (yEnd, yStart);
            if (zStart > zEnd) (zStart, zEnd) = (zEnd, zStart);
            for (int x = xStart; x <= xEnd; x++)
            for (int y = yStart; y <= yEnd; y++)
            for (int z = zStart; z <= zEnd; z++) {
                Vector3Int pos = new Vector3Int(x, y, z);
                if (!blockMap.TryGetValue(pos, out var blocks)) continue;
                foreach (var kvp in blocks)
                foreach (var facing in kvp.Value) {
                    var key = new BlockKey(kvp.Key, pos, facing);
                    if (!blockBoundaryIds.Remove(key, out var partId)) continue;
                    partMap?.RemoveBounds(partId);
                }
            }
        }

        public bool RemoveBlock<T>(T blockType, Vector3Int removeAt) where T : Enum {
            int blockId = Convert.ToInt32(blockType);
            if (!blockMap.TryGetValue(removeAt, out var blocks)) return false;
            if (!blocks.Remove(blockId, out HashSet<Facing> facings)) return false;

            foreach (var facing in facings)
                removeInstanceAndPool(blockId, removeAt, facing, null);

            if (blocks.Count == 0) {
                blockMap.Remove(removeAt);
                everythingPool.DeactivateObject(blocks);
            }

            return true;
        }

        public int RemoveBlocks(Vector3Int at, HashSet<BlockKey> removed) => RemoveBlocks(at, at, removed);

        public int RemoveBlocks<T>(T blockType, Vector3Int from, Vector3Int to, HashSet<BlockKey> removed) {
            removed?.Clear();

            int blockId = Convert.ToInt32(blockType);
            normalizeBox(from, to, out int xStart, out int xEnd, out int yStart, out int yEnd, out int zStart,
                out int zEnd);

            int removedCount = 0;
            for (int x = xStart; x <= xEnd; x++)
            for (int y = yStart; y <= yEnd; y++)
            for (int z = zStart; z <= zEnd; z++) {
                var pos = new Vector3Int(x, y, z);

                if (!blockMap.TryGetValue(pos, out var blocks)) continue;
                if (!blocks.Remove(blockId, out HashSet<Facing> facings)) continue;

                foreach (var facing in facings) {
                    if (!removeInstanceAndPool(blockId, pos, facing, null)) continue;
                    removedCount++;
                    removed?.Add(new BlockKey(blockId, pos, facing));
                }

                if (blocks.Count != 0) continue;
                blockMap.Remove(pos);
                everythingPool.DeactivateObject(blocks);
            }

            return removedCount;
        }

        public int RemoveBlocks<T>(T blockType, Vector3Int from, Vector3Int to, Facing targetFacing,
            HashSet<BlockKey> removed) {
            removed?.Clear();

            int blockId = Convert.ToInt32(blockType);
            normalizeBox(from, to, out int xStart, out int xEnd, out int yStart, out int yEnd, out int zStart,
                out int zEnd);

            int removedCount = 0;
            for (int x = xStart; x <= xEnd; x++)
            for (int y = yStart; y <= yEnd; y++)
            for (int z = zStart; z <= zEnd; z++) {
                var pos = new Vector3Int(x, y, z);

                if (!blockMap.TryGetValue(pos, out var blocks)) continue;
                if (!blocks.TryGetValue(blockId, out HashSet<Facing> facings)) continue;
                if (!facings.Remove(targetFacing)) continue;

                if (!removeInstanceAndPool(blockId, pos, targetFacing, null)) continue; 
                removedCount++; 
                removed?.Add(new BlockKey(blockId, pos, targetFacing));
                
                if (facings.Count == 0) blocks.Remove(blockId);

                if (blocks.Count != 0) continue;
                blockMap.Remove(pos);
                everythingPool.DeactivateObject(blocks);
            }

            return removedCount;
        }

        public int RemoveBlocks(Vector3Int from, Vector3Int to, HashSet<BlockKey> removed) {
            if (!everythingPool)
                throw new ArgumentException("Must set EverythingPool before use by setting BlockMap.everythingPool");

            removed?.Clear();
            normalizeBox(from, to, out int xStart, out int xEnd, out int yStart, out int yEnd, out int zStart,
                out int zEnd);

            int removedCount = 0;
            List<int> tmpIds = everythingPool.ActivateObject<List<int>>();
            List<HashSet<Facing>> tmpFacings = everythingPool.ActivateObject<List<HashSet<Facing>>>();
            try {
                for (int x = xStart; x <= xEnd; x++)
                for (int y = yStart; y <= yEnd; y++)
                for (int z = zStart; z <= zEnd; z++) {
                    var pos = new Vector3Int(x, y, z);

                    if (!blockMap.TryGetValue(pos, out var blocks) || blocks == null || blocks.Count == 0)
                        continue;

                    tmpIds.Clear();
                    tmpFacings.Clear();
                    foreach (var kvp in blocks) {
                        tmpIds.Add(kvp.Key);
                        tmpFacings.Add(kvp.Value);
                    }

                    blocks.Clear();

                    // 逐个回收实例 + 记录 removed
                    for (int i = 0; i < tmpIds.Count; i++) {
                        int id = tmpIds[i];
                        HashSet<Facing> facings = tmpFacings[i];
                        foreach (var facing in facings) {
                            if (!removeInstanceAndPool(id, pos, facing, null)) continue;
                            removedCount++;
                            removed?.Add(new BlockKey(id, pos, facing));
                        }
                    }

                    blockMap.Remove(pos);
                    everythingPool.DeactivateObject(blocks);
                }
            }
            finally {
                everythingPool.DeactivateObject(tmpIds);
                everythingPool.DeactivateObject(tmpFacings);
            }

            return removedCount;
        }

        private static void normalizeBox(Vector3Int a, Vector3Int b, out int xStart, out int xEnd, out int yStart,
            out int yEnd, out int zStart, out int zEnd) {
            xStart = a.x;
            xEnd = b.x;
            yStart = a.y;
            yEnd = b.y;
            zStart = a.z;
            zEnd = b.z;
            if (xStart > xEnd) (xStart, xEnd) = (xEnd, xStart);
            if (yStart > yEnd) (yStart, yEnd) = (yEnd, yStart);
            if (zStart > zEnd) (zStart, zEnd) = (zEnd, zStart);
        }

        private bool removeInstanceAndPool(int blockId, Vector3Int pos, Facing facing, HashSet<Facing> facings) {
            if (!blockInstances.TryGetValue(blockId, out var instMap)) return false;
            if (!instMap.Remove(new BlockKey(blockId, pos, facing), out GameObject inst)) return false;

            releaseBlockData(blockId, pos, facing);
            if (blockBoundaryIds.Remove(new BlockKey(blockId, pos, facing), out int boundaryId))
                partMap?.RemoveBounds(boundaryId);

            facings?.Remove(facing);
            
            if (!ReferenceEquals(inst, null)) 
                instance2Key.Remove(inst);

            if (!inst) return true;
            inst.SetActive(false);

            if (!blockPool.TryGetValue(blockId, out var stack)) {
                stack = new Stack<GameObject>();
                blockPool[blockId] = stack;
            }

            inst.transform.localScale = 1f.fff();
            inst.transform.localEulerAngles = 0f.fff();
            stack.Push(inst);
            return true;
        }

        private void releaseBlockData(int blockId, Vector3Int pos, Facing facing) {
            var key = new BlockKey(blockId, pos, facing);

            if (!blockData.TryGetValue(key, out var dataDict) || dataDict == null)
                return;

            blockData.Remove(key);
            everythingPool.DeactivateObject(dataDict);
        }


        public bool GetBlocksAt(Vector3Int at, HashSet<BlockKey> blockKeys) {
            if (!blockMap.TryGetValue(at, out var blocks) || blocks == null || blocks.Count == 0)
                return false;

            blockKeys?.Clear();
            if (blockKeys == null) return true;

            foreach (var kvp in blocks)
            foreach (var facing in kvp.Value)
                blockKeys.Add(new BlockKey(kvp.Key, at, facing));
            return true;
        }

        public bool GetBlocksAtTop(Vector2Int at, HashSet<BlockKey> blockKeys) {
            int bestY = int.MinValue;
            Dictionary<int, HashSet<Facing>> bestBlocks = null;
            blockKeys?.Clear();

            foreach (var kvp in blockMap) {
                var pos = kvp.Key;
                if (pos.x != at.x || pos.z != at.y) continue;

                var blocks = kvp.Value;
                if (blocks == null || blocks.Count == 0) continue;

                if (pos.y > bestY) {
                    bestY = pos.y;
                    bestBlocks = blocks;
                }
            }

            if (bestBlocks == null) return false;

            if (blockKeys == null) return true;
            foreach (var kvp in bestBlocks)
            foreach (var facing in kvp.Value)
                blockKeys.Add(new BlockKey(kvp.Key, new Vector3Int(at.x, bestY, at.y), facing));

            return true;
        }

        public bool GetBlocks(HashSet<BlockKey> blocks) {
            blocks?.Clear();

            if (blockMap == null || blockMap.Count == 0)
                return false;

            bool any = false;

            foreach (var kvp in blockMap) {
                var pos = kvp.Key;
                var keys = kvp.Value;
                if (keys == null || keys.Count == 0) continue;

                any = true;

                // 只问“有没有”
                if (blocks == null) return true;

                foreach (var kvp1 in keys)
                foreach (var facing in kvp1.Value)
                    blocks.Add(new BlockKey(kvp1.Key, pos, facing));
            }

            return any;
        }

        public bool GetBlocks(Vector3Int from, Vector3Int to, HashSet<BlockKey> blocks) {
            blocks?.Clear();

            normalizeBox(from, to,
                out int xStart, out int xEnd,
                out int yStart, out int yEnd,
                out int zStart, out int zEnd);

            bool any = false;

            for (int x = xStart; x <= xEnd; x++)
            for (int y = yStart; y <= yEnd; y++)
            for (int z = zStart; z <= zEnd; z++) {
                var pos = new Vector3Int(x, y, z);

                if (!blockMap.TryGetValue(pos, out var keys) || keys == null || keys.Count == 0)
                    continue;

                any = true;

                // 只问“有没有”，不需要收集
                if (blocks == null) return true;

                foreach (var kvp in keys)
                foreach (var facing in kvp.Value)
                    blocks.Add(new BlockKey(kvp.Key, pos, facing));
            }

            return any;
        }

        public bool GetBlocksOfType<T>(T blockType, Vector3Int from, Vector3Int to, Facing facing,
            HashSet<BlockKey> blockPositions)
            where T : Enum {
            blockPositions?.Clear();

            int blockId = Convert.ToInt32(blockType);
            if (!blockInstances.TryGetValue(blockId, out var instMap) || instMap == null || instMap.Count == 0)
                return false;

            normalizeBox(from, to,
                out int xStart, out int xEnd,
                out int yStart, out int yEnd,
                out int zStart, out int zEnd);

            bool any = false;

            foreach (var kvp in instMap) {
                var pos = kvp.Key.Position;
                if (pos.x < xStart || pos.x > xEnd) continue;
                if (pos.y < yStart || pos.y > yEnd) continue;
                if (pos.z < zStart || pos.z > zEnd) continue;
                if (kvp.Key.Facing != facing) continue;

                any = true;

                // 只问“有没有”，不需要收集
                if (blockPositions == null) return true;

                blockPositions.Add(kvp.Key);
            }

            return any;
        }


        public Vector3 GetCellWorldPos(Vector3Int gridPos) {
            Vector3 localPos = new Vector3(
                gridPos.x * unitLength,
                gridPos.y * unitLength,
                gridPos.z * unitLength
            );
            return transform.TransformPoint(localPos);
        }

        public Bounds GetCellWorldPosBoundary(Vector3Int gridPos) {
            Bounds b = new Bounds(GetCellWorldPos(gridPos), Vector3.one * unitLength);
            return b;
        }

        public Vector3 GetCellWorldPosCorner2(Vector3Int gridPos, Facing facing1, Facing facing2) {
            Facing? ns = null;
            Facing? ew = null;
            if (facing1 is Facing.north or Facing.south) ns = facing1;
            if (facing2 is Facing.north or Facing.south) ns = facing2;
            if (facing1 is Facing.east or Facing.west) ew = facing1;
            if (facing2 is Facing.east or Facing.west) ew = facing2;
            if (ns == null || ew == null)
                throw new ArgumentException("Must specify exactly two Facing directions! North/South & East/West!");

            Vector3 direction = getUnitVector(ns.Value) + getUnitVector(ew.Value);
            Vector3 worldPos = GetCellWorldPos(gridPos);
            Vector3 cornerPos = worldPos + direction * unitLength / 2f;
            return cornerPos;
        }

        private Vector3 getUnitVector(Facing facing) {
            return facing switch {
                Facing.north => Vector3.forward,
                Facing.south => Vector3.back,
                Facing.east => Vector3.right,
                Facing.west => Vector3.left,
                _ => throw new ArgumentException("Invalid Facing direction: " + facing)
            };
        }

#if UNITY_EDITOR
        private void OnDrawGizmos() {
            BlockGridGizmos.Draw(transform, unitLength, blockMap, blockData, instance2Key, everythingPool);
            partMap?.DrawAllBoundsGizmos();
        }
#endif

        public void ClearAll() {
            if (!everythingPool)
                throw new ArgumentException("Must set EverythingPool before use by setting BlockMap.everythingPool");

            if (partMap != null && blockBoundaryIds != null && blockBoundaryIds.Count != 0) {
                foreach (var kvp in blockBoundaryIds) {
                    // kvp.Value = boundaryId
                    partMap.RemoveBounds(kvp.Value);
                }
            }

            blockBoundaryIds?.Clear();

            if (blockData != null && blockData.Count != 0) {
                foreach (var kvp in blockData) {
                    var dataDict = kvp.Value;
                    if (dataDict == null) continue;
                    dataDict.Clear(); // 释放引用
                    everythingPool.DeactivateObject(dataDict);
                }

                blockData.Clear();
            }

            if (blockMap != null && blockMap.Count != 0) {
                foreach (var kvp in blockMap) {
                    var blocks = kvp.Value;
                    if (blocks == null) continue;

                    // 这些 HashSet 不是池化的，但清一下能释放引用
                    foreach (var set in blocks.Values)
                        set?.Clear();

                    blocks.Clear();
                    everythingPool.DeactivateObject(blocks);
                }

                blockMap.Clear();
            }

            if (blockInstances != null && blockInstances.Count != 0) {
                foreach (var kvp in blockInstances)
                    kvp.Value?.Clear();

                blockInstances.Clear();
            }

            instance2Key?.Clear();
            foreach (var kvp in blockPool)
            foreach (var inst in kvp.Value)
                Destroy(inst);
            blockPool?.Clear();
        }

        private void OnDestroy() => ClearAll();
    }
}