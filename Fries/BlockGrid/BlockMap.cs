using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Fries.BlockGrid.Fries.BlockGrid;
using Fries.BlockGrid.LevelEdit;
using Fries.Data;
using Fries.EvtSystem;
using Fries.Pool;
using UnityEngine;

namespace Fries.BlockGrid {
    public class BlockMap : MonoBehaviour {
        private readonly struct BlockKeyPosIdFacingComparer : IComparer<BlockKey> {
            public static readonly BlockKeyPosIdFacingComparer Instance = new();

            public int Compare(BlockKey a, BlockKey b) {
                var ap = a.Position;
                var bp = b.Position;

                int c = ap.x.CompareTo(bp.x);
                if (c != 0) return c;
                c = ap.y.CompareTo(bp.y);
                if (c != 0) return c;
                c = ap.z.CompareTo(bp.z);
                if (c != 0) return c;

                c = a.BlockTypeId.CompareTo(b.BlockTypeId);
                if (c != 0) return c;
                return ((int)a.Facing).CompareTo((int)b.Facing);
            }
        }
        
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

        [EvtDeclarer] public struct OnBlockMapInit { BlockMap blockMap; }

        private EverythingPool _everythingPool;

        public EverythingPool everythingPool {
            private get => _everythingPool;
            set {
                if (_everythingPool) throw new InvalidOperationException("Cannot set EverythingPool twice!");
                partMap = new IrregularPartMap(UnitLength, value, transform);
                _everythingPool = value;
            }
        }
        public bool HasEverythingPool => _everythingPool;

        public IrregularPartMap partMap { get; private set; }

        private void Awake() {
            Evt.TriggerNonAlloc<OnBlockMapInit>(this);
        }

        [SerializeField] private float unitLength = 1f;
        public float UnitLength => unitLength;
        internal Dictionary<Vector3Int, Dictionary<int, ListSet<Facing>>> blockMap = new();
        private Dictionary<int, Dictionary<BlockKey, GameObject>> blockInstances = new();
        private Dictionary<int, Stack<GameObject>> blockPool = new();
        private Dictionary<BlockKey, int> blockBoundaryIds = new();
        private Dictionary<GameObject, BlockKey> instance2Key = new();
        private Dictionary<BlockKey, Dictionary<int, object>> blockData = new();
        internal Dictionary<Vector2Int, int> xz2TopY = new();
        private void tryRecordTopBlock(Vector3Int pos) {
            bool shouldUpdate = false;
            
            Vector2Int xz = new(pos.x, pos.z);
            if (!xz2TopY.TryGetValue(xz, out int topY)) shouldUpdate = true;
            else if (topY < pos.y) shouldUpdate = true;
            
            if (shouldUpdate) xz2TopY[xz] = pos.y;
        }

        public readonly List<(int, object)> CustomDataRegister = new();

        public void SetBlock<T>(Vector3Int at, T blkType, Facing direction = Facing.north, 
            bool writeToPartMap = false, Action<GameObject, BlockKey> onBlockCreation = null)
            where T : Enum {
            SetBlock(at, at, blkType, direction, writeToPartMap);
        }

        public bool TryGetData(BlockKey key, out Dictionary<int, object> dataDict) =>
            blockData.TryGetValue(key, out dataDict);
        
        public void SetData(BlockKey key, params (int, object)[] data) {
            if (!(blockMap.TryGetValue(key.Position, out var blocks) &&
                  blocks.TryGetValue(key.BlockTypeId, out var facings) && facings.Contains(key.Facing))) {
                Debug.LogError("Cannot set data for block " + key + " because it does not exist!");
                return;
            }

            if (!blockData.TryGetValue(key, out var dataDict)) 
                blockData[key] = dataDict = everythingPool.ActivateObject<Dictionary<int, object>>();
            foreach (var valueTuple in data) dataDict[valueTuple.Item1] = valueTuple.Item2;
        }

        public void RemoveAllData(BlockKey key, Dictionary<int, object> dataDictOut = null) {
            if (!(blockMap.TryGetValue(key.Position, out var blocks) &&
                  blocks.TryGetValue(key.BlockTypeId, out var facings) && facings.Contains(key.Facing))) {
                Debug.LogError("Cannot remove data for block " + key + " because it does not exist!");
                return;
            }

            if (!blockData.TryGetValue(key, out var dataDict)) return;
            foreach (var valueTuple in dataDict) dataDictOut?.Add(valueTuple.Key, valueTuple.Value);
            everythingPool.DeactivateObject(dataDict);
            blockData.Remove(key);
        }
        public void RemoveData(BlockKey key, Dictionary<int, object> dataDictOut = null, params int[] dataIds) {
            if (!(blockMap.TryGetValue(key.Position, out var blocks) &&
                  blocks.TryGetValue(key.BlockTypeId, out var facings) && facings.Contains(key.Facing))) {
                Debug.LogError("Cannot remve data for block " + key + " because it does not exist!");
                return;
            }
            
            if (!blockData.TryGetValue(key, out var dataDict)) return;
            foreach (var dataId in dataIds) {
                if (dataDict.Remove(dataId, out var value))
                    dataDictOut?.Add(dataId, value);
            }

            if (dataDict.Count != 0) return;
            everythingPool.DeactivateObject(dataDict);
            blockData.Remove(key);
        }

        private Dictionary<BlockKey, Color> dyedBlocks = new();
        public void DyeData(int dataId, Color color) {
            foreach (var kvp in blockData) {
                if (kvp.Value.ContainsKey(dataId)) 
                    dyedBlocks[kvp.Key] = color;
            }
        }
        public void WashDye() => dyedBlocks.Clear();

        public const int FACING = 0;

        public void SetBlock<T>(Vector3Int pos1, Vector3Int pos2, T blockType, Facing direction = Facing.north,
            bool writeToPartMap = false, Action<GameObject, BlockKey> onBlockCreation = null) where T : Enum {
            SetBlock(pos1, pos2, (object)blockType, direction, writeToPartMap);
        }

        internal void OverwriteSetBlock(Schematic schematic, ListSet<BlockKey> original, bool writeToPartMap = false) {
            int xStart = schematic.pos1.x;
            int yStart = schematic.pos1.y;
            int zStart = schematic.pos1.z;
            int xEnd = schematic.pos2.x;
            int yEnd = schematic.pos2.y;
            int zEnd = schematic.pos2.z;
            if (xStart > xEnd) (xStart, xEnd) = (xEnd, xStart);
            if (yStart > yEnd) (yStart, yEnd) = (yEnd, yStart);
            if (zStart > zEnd) (zStart, zEnd) = (zEnd, zStart);
            
            int xLength = xEnd - xStart + 1;
            int yLength = yEnd - yStart + 1;
            int zLength = zEnd - zStart + 1;

            var currentBlockGroupBlockType = everythingPool.ActivateObject<List<(int, Facing)>>();

            try {
                schematic.ResetBlockGroupIndex();
                int amountOfBlocks = schematic.NextCompressedBlockGroup(currentBlockGroupBlockType, out bool succeed);
                if (!succeed) return;

                bool noMoreGroups = false;
                
                for (int x = 0; x < xLength; x++)
                for (int y = 0; y < yLength; y++)
                for (int z = 0; z < zLength; z++) {
                    // 确保当前 CompressedBlockGroup 的待放置长度不为 0
                    if (!noMoreGroups) {
                        while (amountOfBlocks <= 0) {
                            currentBlockGroupBlockType.Clear();
                            amountOfBlocks =
                                schematic.NextCompressedBlockGroup(currentBlockGroupBlockType, out bool isSucceed);
                            // 如果列表已经被获取空，则说明放置完成，那么返回
                            if (!isSucceed) {
                                noMoreGroups = true;
                                amountOfBlocks = 0;
                                break;
                            }
                        }
                    }

                    Vector3Int pos = new Vector3Int(xStart + x, yStart + y, zStart + z);

                    // 记录这里原先的方块类型
                    if (blockMap.TryGetValue(pos, out var blocks1)) {
                        foreach (var (blockId, facings1) in blocks1.ToList()) {
                            foreach (var facing in facings1.ToList()) {
                                BlockKey key = new BlockKey(blockId, pos, facing);
                                if (removeBlocks(blockId, pos, facing)) original.Add(key);
                                else {
                                    Debug.LogError($"removeBlocks failed for {key}, aborting overwrite to keep consistency.");
                                    return;
                                }
                            }
                        }
                    }

                    // 如果这一组 CompressedBlockGroup 已经全放置完了，在删除本块上的方块数据后，就可以不继续往下走了
                    if (noMoreGroups) continue;

                    // 遍历该坐标上要放置的每个方块种类
                    foreach (var (blockId, facing) in currentBlockGroupBlockType) {
                        BlockKey key = new BlockKey(blockId, pos, facing);

                        // 如果需要，把该方块的 Bounds 写入到 PartMap 里
                        if (writeToPartMap) {
                            int id = partMap.AddBounds(GetCellWorldPosBoundary(pos));
                            blockBoundaryIds[key] = id;
                        }

                        // 获取预制体
                        object blockType = BlockRegistry.GetEnum(blockId);
                        string prefabPath = BlockRegistry.GetPath(blockType, out int _);
                        GameObject prefab = findPrefab(blockId, prefabPath);
                        if (!prefab) throw new FileNotFoundException($"There is no prefab on path {prefabPath}!");

                        // 获取方块实例
                        GameObject inst = null;
                        if (!blockPool.TryGetValue(blockId, out Stack<GameObject> pool))
                            blockPool[blockId] = new Stack<GameObject>();
                        else if (pool.Count != 0)
                            inst = pool.Pop();
                        if (!inst) inst = Instantiate(prefab);

                        // 初始化方块状态
                        inst.transform.SetParent(transform, false);
                        inst.transform.localScale = 1f.fff();
                        inst.transform.localEulerAngles = 0f.fff();
                        DirectioonalBlockApplier.apply(blockType, inst.transform, facing);
                        inst.transform.localPosition = prefab.transform.localPosition +
                                                       new Vector3(pos.x * unitLength, pos.y * unitLength,
                                                           pos.z * unitLength);
                        inst.SetActive(true);

                        // 准备开始写入 BlockMap 数据
                        // 获取或创建 BlockMap 项
                        if (!blockMap.TryGetValue(pos, out var blocks)) {
                            blocks = everythingPool.ActivateObject<Dictionary<int, ListSet<Facing>>>();
                            blockMap[pos] = blocks;
                        }

                        // 获取或创建 BlockMap项项
                        if (!blocks.TryGetValue(blockId, out var facings)) {
                            facings = everythingPool.ActivateObject<ListSet<Facing>>();
                            blocks.Add(blockId, facings);
                        }

                        // 登记顶部方块信息
                        tryRecordTopBlock(pos);
                        // 将方块数据写入 BlockMap
                        facings.Add(facing);

                        // 在这里编辑 Block 的 CustomData {
                        if (CustomDataRegister.Count > 0) {
                            if (blockData.TryGetValue(key, out var dataDictOld))
                                everythingPool.DeactivateObject(dataDictOld);
                            var dataDict = everythingPool.ActivateObject<Dictionary<int, object>>();
                            blockData[key] = dataDict;
                            dataDict[FACING] = facing;
                            foreach (var tuple in CustomDataRegister)
                                dataDict[tuple.Item1] = tuple.Item2;
                        }
                        // }

                        // 获取或创建 BlockInstance 项
                        if (!blockInstances.TryGetValue(blockId, out var dict)) {
                            dict = new Dictionary<BlockKey, GameObject>();
                            blockInstances[blockId] = dict;
                        }

                        // 记录 BlockInstance
                        if (instance2Key.TryGetValue(inst, out var oldKey)) {
                            dict.Remove(oldKey);
                            instance2Key.Remove(inst);
                        }
                        dict[key] = inst;
                        instance2Key[inst] = key;
                    }

                    amountOfBlocks--;
                }
            }
            finally {
                everythingPool.DeactivateObject(currentBlockGroupBlockType);
            }
        }

        internal void SetBlock(Vector3Int at, object blockType, Facing direction = Facing.north,
            bool writeToPartMap = false, Action<GameObject, BlockKey> onBlockCreation = null) {
            SetBlock(at, at, blockType, direction, writeToPartMap, onBlockCreation);
        }

        internal void SetBlock(Vector3Int pos1, Vector3Int pos2, object blockType, Facing direction = Facing.north,
            bool writeToPartMap = false, Action<GameObject, BlockKey> onBlockCreation = null,
            bool createBlock = true, GameObject blockInst = null) {
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

                if (hasBlocksOfType(blockId, pos, direction))
                    removeBlocks(blockId, pos, direction);

                if (writeToPartMap) {
                    int id = partMap.AddBounds(GetCellWorldPosBoundary(pos));
                    blockBoundaryIds[key] = id;
                }

                GameObject inst = null;
                prefab ??= findPrefab(blockId, prefabPath);
                if (!prefab) throw new FileNotFoundException($"There is no prefab on path {prefabPath}!");

                if (createBlock) {
                    if (!blockPool.TryGetValue(blockId, out Stack<GameObject> pool))
                        blockPool[blockId] = new Stack<GameObject>();
                    else if (pool.Count != 0)
                        inst = pool.Pop();

                    if (!inst) inst = Instantiate(prefab);
                }
                else {
                    inst = blockInst;
                    if (!inst) 
                        throw new ArgumentException("BlockInst must be set when createBlock is false! This is a severe error, block data will not be consistent!"); 
                }

                inst.transform.SetParent(transform, false);
                inst.transform.localScale = 1f.fff();
                inst.transform.localEulerAngles = 0f.fff();
                DirectioonalBlockApplier.apply(blockType, inst.transform, direction);
                inst.transform.localPosition = prefab.transform.localPosition +
                                               new Vector3(x * unitLength, y * unitLength, z * unitLength);
                inst.SetActive(true);
                
                onBlockCreation?.Invoke(inst, key);

                if (!blockMap.TryGetValue(pos, out var blocks)) {
                    blocks = everythingPool.ActivateObject<Dictionary<int, ListSet<Facing>>>();
                    blockMap[pos] = blocks;
                }

                if (!blocks.TryGetValue(blockId, out var facings)) {
                    facings = everythingPool.ActivateObject<ListSet<Facing>>();
                    blocks.Add(blockId, facings);
                }
                tryRecordTopBlock(pos);

                facings.Add(direction);

                // 在这里添加了 Block 的 CustomData 的编辑 {
                if (CustomDataRegister.Count > 0) {
                    if (blockData.TryGetValue(key, out var dataDictOld))
                        everythingPool.DeactivateObject(dataDictOld);
                    var dataDict = everythingPool.ActivateObject<Dictionary<int, object>>();
                    blockData[key] = dataDict;
                    dataDict[FACING] = direction;
                    foreach (var tuple in CustomDataRegister)
                        dataDict[tuple.Item1] = tuple.Item2;
                }
                // }

                if (!blockInstances.TryGetValue(blockId, out var dict)) {
                    dict = new Dictionary<BlockKey, GameObject>();
                    blockInstances[blockId] = dict;
                }

                if (instance2Key.TryGetValue(inst, out var oldKey)) {
                    dict.Remove(oldKey);
                    instance2Key.Remove(inst);
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

        public int RemoveBlocks(ISet<BlockKey> shouldBeRemoved, ISet<BlockKey> removed) {
            int result = 0;
            removed?.Clear();
            if (shouldBeRemoved == null || shouldBeRemoved.Count == 0) return 0;

            if (!everythingPool)
                throw new ArgumentException("Must set EverythingPool before use by setting BlockMap.everythingPool");

            // 1) 拷贝 + 排序：让 (pos, blockId) 连续，减少 blockMap / blocks / blockInstances 的哈希查找
            var keys = everythingPool.ActivateObject<List<BlockKey>>();
            keys.Clear();
            foreach (var k in shouldBeRemoved) keys.Add(k);
            keys.Sort(BlockKeyPosIdFacingComparer.Instance);

            Vector3Int curPos = default;
            bool hasPos = false;
            Dictionary<int, ListSet<Facing>> curBlocks = null;

            int curBlockId = 0;
            bool hasBlockId = false;
            ListSet<Facing> curFacings = null;

            Dictionary<BlockKey, GameObject> curInstMap = null;
            bool curInstMapValid = false;

            Stack<GameObject> curPoolStack = null;

            void FlushCurrentBlockIdGroup() {
                if (curBlocks == null || !hasBlockId) return;

                // facings 被删空：移除 blockId 项，并回收 facings 容器
                if (curFacings != null && curFacings.Count == 0) {
                    curBlocks.Remove(curBlockId);
                    curFacings.Clear();
                    everythingPool.DeactivateObject(curFacings);
                }

                curFacings = null;
                curInstMap = null;
                curInstMapValid = false;
                curPoolStack = null;
                hasBlockId = false;

                // blocks 被删空：移除 pos 项，并回收 blocks 容器
                if (curBlocks.Count == 0) {
                    blockMap.Remove(curPos);
                    curBlocks.Clear();
                    everythingPool.DeactivateObject(curBlocks);
                    curBlocks = null;
                    hasPos = false;
                }
            }

            void FlushCurrentPosGroup() {
                // pos 切换时，先把当前 blockId 组 flush 掉
                FlushCurrentBlockIdGroup();
            }

            try {
                for (int i = 0; i < keys.Count; i++) {
                    var key = keys[i];
                    var pos = key.Position;
                    int blockId = key.BlockTypeId;
                    var facing = key.Facing;

                    // pos 变了：flush 旧 pos，并抓取新 pos 的 blocks（只做一次哈希查找）
                    if (!hasPos || pos != curPos) {
                        FlushCurrentPosGroup();

                        curPos = pos;
                        hasPos = true;

                        if (!blockMap.TryGetValue(curPos, out curBlocks) || curBlocks == null || curBlocks.Count == 0) {
                            curBlocks = null;
                            hasPos = false;
                            continue;
                        }

                        // 新 pos 开始，blockId group 尚未建立
                        hasBlockId = false;
                        curFacings = null;
                        curInstMap = null;
                        curInstMapValid = false;
                        curPoolStack = null;
                    }

                    // blockId 变了：flush 旧 blockId，并抓取新 blockId 的 facings / instMap（各只做一次哈希查找）
                    if (!hasBlockId || blockId != curBlockId) {
                        FlushCurrentBlockIdGroup();

                        curBlockId = blockId;
                        hasBlockId = true;

                        if (!curBlocks.TryGetValue(curBlockId, out curFacings) || curFacings == null ||
                            curFacings.Count == 0) {
                            curFacings = null;
                            hasBlockId = false;
                            continue;
                        }

                        curInstMapValid = blockInstances.TryGetValue(curBlockId, out curInstMap) && curInstMap != null;
                        curPoolStack = null; // 用到再取，避免多余哈希查找
                    }

                    // 先从 facings 里移除（ListSet 线性，不是哈希）
                    if (!curFacings.Remove(facing))
                        continue;

                    // ---- 下面开始做“每个 key 必须做”的哈希移除（无法再少了） ----

                    // 1) blockData：Remove(key, out value) 只做一次哈希
                    if (blockData.Remove(key, out var dataDict) && dataDict != null) {
                        dataDict.Clear(); // 释放引用，避免池里残留强引用
                        everythingPool.DeactivateObject(dataDict);
                    }

                    // 2) partMap boundary：Remove(key, out id) 只做一次哈希
                    if (blockBoundaryIds.Remove(key, out int boundaryId))
                        partMap?.RemoveBounds(boundaryId);

                    // 3) 实例表：用缓存的 instMap，避免每次都 blockInstances.TryGetValue(blockId)
                    GameObject inst = null;
                    bool instRemoved = curInstMapValid && curInstMap.Remove(key, out inst);

                    // 如果实例表里没有（理论上不该发生），保持和你其它 Remove* 的语义一致：不计入 removed
                    if (!instRemoved)
                        continue;

                    // instance2Key：只有拿到了 inst 才需要 hash remove
                    if (!ReferenceEquals(inst, null)) instance2Key.Remove(inst);

                    // Unity destroyed 物体：inst == false，但我们仍然视为“已从表中移除”
                    if (!inst) {
                        result++;
                        removed?.Add(key);
                        continue;
                    }

                    inst.SetActive(false);

                    // pool：按 blockId 缓存 stack，避免每个 key 都 blockPool.TryGetValue
                    if (curPoolStack == null) {
                        if (!blockPool.TryGetValue(curBlockId, out curPoolStack)) {
                            curPoolStack = new Stack<GameObject>();
                            blockPool[curBlockId] = curPoolStack;
                        }
                    }

                    inst.transform.localScale = 1f.fff();
                    inst.transform.localEulerAngles = 0f.fff();
                    curPoolStack.Push(inst);

                    result++;
                    removed?.Add(key);
                }

                // 最后把尾巴 flush 掉
                FlushCurrentPosGroup();
            }
            finally {
                keys.Clear();
                everythingPool.DeactivateObject(keys);
            }

            return result;
        }

        private bool removeBlocks(int blockTypeId, Vector3Int at, Facing targetFacing) {
            var pos = at;

            if (!blockMap.TryGetValue(pos, out var blocks)) return false;
            if (!blocks.TryGetValue(blockTypeId, out ListSet<Facing> facings)) return false;
            if (!facings.Remove(targetFacing)) return false;

            if (!removeInstanceAndPool(blockTypeId, pos, targetFacing)) return false;

            if (facings.Count == 0) {
                blocks.Remove(blockTypeId);
                everythingPool.DeactivateObject(facings);
            }

            if (blocks.Count == 0) {
                blockMap.Remove(pos);
                everythingPool.DeactivateObject(blocks);
            }

            return true;
        }

        private bool removeInstanceAndPool(int blockId, Vector3Int pos, Facing facing) {
            if (!blockInstances.TryGetValue(blockId, out var instMap)) return false;
            if (!instMap.Remove(new BlockKey(blockId, pos, facing), out GameObject inst)) return false;

            releaseBlockData(blockId, pos, facing);
            if (blockBoundaryIds.Remove(new BlockKey(blockId, pos, facing), out int boundaryId))
                partMap?.RemoveBounds(boundaryId);

            if (!ReferenceEquals(inst, null)) {
                if (instance2Key.TryGetValue(inst, out var key)) {
                    instMap.Remove(key);
                    instance2Key.Remove(inst);
                }
            }

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

        private bool hasBlocksOfType(int blockTypeId, Vector3Int at, Facing facing) {
            if (!blockInstances.TryGetValue(blockTypeId, out var instMap) || instMap == null || instMap.Count == 0)
                return false;
            return instMap.ContainsKey(new BlockKey(blockTypeId, at, facing));
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
            BlockGridGizmos.Draw(transform, unitLength, blockMap, blockData, instance2Key, dyedBlocks, GetCellWorldPos, everythingPool);
            partMap?.DrawAllBoundsGizmos();
        }
#endif

        public void ClearAll() {
            if (!everythingPool) return;
            
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

                    foreach (var set in blocks.Values)
                        everythingPool.DeactivateObject(set);
                    
                    blocks.Clear();
                    everythingPool.DeactivateObject(blocks);
                }

                blockMap.Clear();
            }

            if (instance2Key != null && instance2Key.Count != 0) {
                foreach (var kvp in instance2Key) {
                    var inst = kvp.Key;
                    if (inst) Destroy(inst);
                }
                instance2Key.Clear();
            }
            
            if (blockInstances != null && blockInstances.Count != 0) {
                foreach (var kvp in blockInstances)
                    kvp.Value?.Clear();

                blockInstances.Clear();
            }

            foreach (var kvp in blockPool)
            foreach (var inst in kvp.Value)
                Destroy(inst);
            blockPool?.Clear();
        }

        private void OnDestroy() => ClearAll();

        public Vector3Int GetCellPos(Vector3 position) {
            if (unitLength <= 0f)
                throw new InvalidOperationException("unitLength must be > 0");

            Vector3 local = transform.InverseTransformPoint(position);
            float inv = 1f / unitLength;

            static int RoundHalfAwayFromZero(float v) =>
                v >= 0f ? Mathf.FloorToInt(v + 0.5f) : Mathf.CeilToInt(v - 0.5f);

            int x = RoundHalfAwayFromZero(local.x * inv);
            int y = RoundHalfAwayFromZero(local.y * inv);
            int z = RoundHalfAwayFromZero(local.z * inv);

            return new Vector3Int(x, y, z);
        }
    }
}