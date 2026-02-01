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
        private Dictionary<Vector3Int, HashSet<int>> blockMap = new();
        private Dictionary<int, Dictionary<Vector3Int, GameObject>> blockInstances = new();
        private Dictionary<int, Stack<GameObject>> blockPool = new();
        private Dictionary<Vector3Int, HashSet<int>> blockBoundaryIds = new();

        public void SetBlock<T>(Vector3Int at, T blkType, Facing direction = Facing.north, bool writeToPartMap = false)
            where T : Enum {
            SetBlock(at, at, blkType, direction, writeToPartMap);
        }

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

                if (writeToPartMap) {
                    int id = partMap.AddBounds(GetCellWorldPosBoundary(pos));
                    if (!blockBoundaryIds.TryGetValue(pos, out var set)) {
                        set = everythingPool.ActivateObject<HashSet<int>>();
                        blockBoundaryIds[pos] = set;
                    }

                    set.Add(id);
                }

                GameObject inst = null;

                if (GetBlocksAt(pos, null)) RemoveBlocks(pos, null);

                if (!blockPool.TryGetValue(blockId, out Stack<GameObject> pool))
                    blockPool[blockId] = new Stack<GameObject>();
                else if (pool.Count != 0) {
                    inst = pool.Pop();
                    inst.SetActive(true);
                }

                prefab ??= findPrefab(blockId, prefabPath);
                if (!prefab) throw new FileNotFoundException($"There is no prefab on path {prefabPath}!");
                if (!inst) inst = Instantiate(prefab);

                inst.transform.SetParent(transform, false);
                DirectioonalBlockApplier.apply(blockType, inst.transform, direction);
                inst.transform.localPosition = prefab.transform.localPosition +
                                               new Vector3(x * unitLength, y * unitLength, z * unitLength);

                if (!blockMap.TryGetValue(pos, out var blocks)) {
                    blocks = everythingPool.ActivateObject<HashSet<int>>();
                    blockMap[pos] = blocks;
                }

                blocks.Add(blockId);

                if (!blockInstances.TryGetValue(blockId, out var dict)) {
                    dict = new Dictionary<Vector3Int, GameObject>();
                    blockInstances[blockId] = dict;
                }

                dict[pos] = inst;
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
                if (!blockBoundaryIds.TryGetValue(pos, out var set)) continue;
                foreach (int id in set) partMap?.RemoveBounds(id);
            }
        }

        public bool RemoveBlock<T>(T blockType, Vector3Int removeAt) where T : Enum {
            int blockId = Convert.ToInt32(blockType);
            if (!blockMap.TryGetValue(removeAt, out var blocks)) return false;
            if (!blocks.Remove(blockId)) return false;

            removeInstanceAndPool(blockId, removeAt);
            if (blocks.Count == 0) {
                blockMap.Remove(removeAt);
                everythingPool.DeactivateObject(blocks);
            }

            return true;
        }

        public int RemoveBlocks(Vector3Int at, HashSet<BlockKey> removed) => RemoveBlocks(at, at, removed);

        public int RemoveBlocks<T>(T blockType, Vector3Int from, Vector3Int to, HashSet<Vector3Int> removed) {
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
                if (!blocks.Remove(blockId)) continue;

                if (removeInstanceAndPool(blockId, pos)) {
                    removedCount++;
                    removed?.Add(pos);
                }

                if (blocks.Count == 0) {
                    blockMap.Remove(pos);
                    everythingPool.DeactivateObject(blocks);
                }
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
            try {
                for (int x = xStart; x <= xEnd; x++)
                for (int y = yStart; y <= yEnd; y++)
                for (int z = zStart; z <= zEnd; z++) {
                    var pos = new Vector3Int(x, y, z);

                    if (!blockMap.TryGetValue(pos, out var blocks) || blocks == null || blocks.Count == 0)
                        continue;

                    tmpIds.Clear();
                    foreach (int id in blocks) tmpIds.Add(id);
                    blocks.Clear();

                    // 逐个回收实例 + 记录 removed
                    foreach (var id in tmpIds) {
                        if (!removeInstanceAndPool(id, pos)) continue;
                        removedCount++;
                        removed?.Add(new BlockKey(id, pos));
                    }

                    blockMap.Remove(pos);
                    everythingPool.DeactivateObject(blocks);
                }
            }
            finally {
                everythingPool.DeactivateObject(tmpIds);
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

        private bool removeInstanceAndPool(int blockId, Vector3Int pos) {
            if (!blockInstances.TryGetValue(blockId, out var instMap)) return false;
            if (!instMap.Remove(pos, out GameObject inst)) return false;
            if (!inst) return true;

            inst.SetActive(false);

            if (!blockPool.TryGetValue(blockId, out var stack)) {
                stack = new Stack<GameObject>();
                blockPool[blockId] = stack;
            }

            stack.Push(inst);
            return true;
        }


        public bool GetBlocksAt(Vector3Int at, HashSet<int> blockTypeIds) {
            if (!blockMap.TryGetValue(at, out var blocks) || blocks == null || blocks.Count == 0)
                return false;

            blockTypeIds?.Clear();
            if (blockTypeIds == null) return true;

            foreach (int id in blocks)
                blockTypeIds.Add(id);
            return true;
        }

        public bool GetBlocksAtTop(Vector2Int at, HashSet<int> blockTypeIds) {
            int bestY = int.MinValue;
            HashSet<int> bestBlocks = null;
            blockTypeIds?.Clear();

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

            if (blockTypeIds == null) return true;
            foreach (int id in bestBlocks)
                blockTypeIds.Add(id);

            return true;
        }
        
        public bool GetBlocks(HashSet<BlockKey> blocks) {
            blocks?.Clear();

            if (blockMap == null || blockMap.Count == 0)
                return false;

            bool any = false;

            foreach (var kvp in blockMap) {
                var pos = kvp.Key;
                var ids = kvp.Value;
                if (ids == null || ids.Count == 0) continue;

                any = true;

                // 只问“有没有”
                if (blocks == null) return true;

                foreach (int id in ids)
                    blocks.Add(new BlockKey(id, pos));
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

                if (!blockMap.TryGetValue(pos, out var ids) || ids == null || ids.Count == 0)
                    continue;

                any = true;

                // 只问“有没有”，不需要收集
                if (blocks == null) return true;

                foreach (int id in ids)
                    blocks.Add(new BlockKey(id, pos));
            }

            return any;
        }

        public bool GetBlocksOfType(Vector3Int from, Vector3Int to, HashSet<Vector3Int> blockPositions) {
            blockPositions?.Clear();

            normalizeBox(from, to,
                out int xStart, out int xEnd,
                out int yStart, out int yEnd,
                out int zStart, out int zEnd);

            bool any = false;

            for (int x = xStart; x <= xEnd; x++)
            for (int y = yStart; y <= yEnd; y++)
            for (int z = zStart; z <= zEnd; z++) {
                var pos = new Vector3Int(x, y, z);

                if (!blockMap.TryGetValue(pos, out var ids) || ids == null || ids.Count == 0)
                    continue;

                any = true;

                // 只问“有没有”，不需要收集
                if (blockPositions == null) return true;

                blockPositions.Add(pos);
            }

            return any;
        }

        public bool GetBlocksOfType<T>(T blockType, Vector3Int from, Vector3Int to, HashSet<Vector3Int> blockPositions)
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
                var pos = kvp.Key;
                if (pos.x < xStart || pos.x > xEnd) continue;
                if (pos.y < yStart || pos.y > yEnd) continue;
                if (pos.z < zStart || pos.z > zEnd) continue;

                any = true;

                // 只问“有没有”，不需要收集
                if (blockPositions == null) return true;

                blockPositions.Add(pos);
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
        [SerializeField] private float gridLength = 1f;
        private void OnDrawGizmos() {
            BlockGridGizmos.Draw(transform, unitLength, blockMap);
            partMap?.DrawAllBoundsGizmos();
        }
#endif
    }
}