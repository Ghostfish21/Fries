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
        public struct OnBlockMapInit { BlockMap blockMap; }
        
        public EverythingPool everythingPool { private get; set; }
        private void Awake() {
            Evt.TriggerNonAlloc<OnBlockMapInit>(this);
        }

        [SerializeField] private float unitLength = 1f;
        private Dictionary<Vector3Int, HashSet<int>> blockMap = new();
        private Dictionary<int, Dictionary<Vector3Int, GameObject>> blockInstances = new();
        private Dictionary<int, Stack<GameObject>> blockPool = new();

        public void SetBlock<T>(Vector3Int at, T blkType, Facing direction = Facing.north) where T : Enum {
            SetBlock(at, at, blkType, direction);
        }
        public void SetBlock<T>(Vector3Int pos1, Vector3Int pos2, T blockType, Facing direction = Facing.north) where T : Enum {
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
            finally { everythingPool.DeactivateObject(tmpIds); }

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

            if (blockTypeIds == null) return true;
            
            foreach (int id in blocks) 
                blockTypeIds.Add(id);
            return true;
        }

        public bool GetBlocksAtTop(Vector2Int at, HashSet<int> blockTypeIds) {
            int bestY = int.MinValue;
            HashSet<int> bestBlocks = null;

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
        
#if UNITY_EDITOR
        [SerializeField] private float gridLength = 1f;
        private void OnDrawGizmos() {
            BlockGridGizmos.Draw(transform, unitLength, blockMap);
        }
#endif
    }
}