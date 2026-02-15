using System;
using System.Collections.Generic;
using System.Text;
using Fries.Data;
using Fries.Pool;
using UnityEngine;

namespace Fries.BlockGrid.LevelEdit {
    public class Schematic {
        private static readonly IComparer<BlockKey> SpaceMinToMaxComparer =
            Comparer<BlockKey>.Create((a, b) => {
                var pa = a.Position;
                var pb = b.Position;
                int c = pa.x.CompareTo(pb.x);
                if (c != 0) return c;
                c = pa.y.CompareTo(pb.y);
                if (c != 0) return c;
                return pa.z.CompareTo(pb.z);
            });
        public void SortBySpaceMinToMax(ISet<BlockKey> input, List<BlockKey> sorted) {
            if (sorted == null) throw new ArgumentNullException(nameof(sorted));
            sorted.Clear();
            if (input == null || input.Count == 0) return;
            if (sorted.Capacity < input.Count) sorted.Capacity = input.Count;
            foreach (var bk in input) sorted.Add(bk);
            sorted.Sort(SpaceMinToMaxComparer);
        }

        private readonly EverythingPool pool;

        public Vector3Int pos1 { get; set; }
        public Vector3Int pos2 { get; set; }
        
        // BlockKey 到 压缩ID 的正反向字典
        // Compressed Id -> Block Key
        private readonly Dictionary<ushort, (int, Facing)> blockTypeLookupTable;
        private readonly Dictionary<(int, Facing), ushort> backwardBlockTypeLookupTable;
        
        // 存储压缩后的方块数据，string 中的每一个 char 代表一个方块 CompressedId 的强转
        // (Compressed Ids, Amount)
        private readonly List<(string, int)> changes;
        
        private Schematic(EverythingPool everythingPool, Vector3Int pos1, Vector3Int pos2) {
            pool = everythingPool;
            this.pos1 = pos1;
            this.pos2 = pos2;
            blockTypeLookupTable = new Dictionary<ushort, (int, Facing)>();
            backwardBlockTypeLookupTable = new Dictionary<(int, Facing), ushort>();
            changes = new List<(string, int)>();
            stringBuilder = new();
            compressedIds = new();
        }

        public const ushort EMPTY = 0;
        public static readonly string EMPTYSTR = ((char)EMPTY).ToString();

        private readonly StringBuilder stringBuilder;
        private readonly List<ushort> compressedIds;
        // 在排序了的方块信息中查询给定坐标上的所有方块，并整理成唯一的 string 表达
        private string getCompressedIdsOnPosition(Vector3Int checkingPos, List<BlockKey> sortedBlockKeys,
            ref int currentIndex, ref ushort incrementalCounter) {
            
            if (sortedBlockKeys.Count == 0) return EMPTYSTR;
            if (currentIndex >= sortedBlockKeys.Count) return EMPTYSTR;

            compressedIds.Clear();
            
            var blockKey = sortedBlockKeys[currentIndex];
            while (checkingPos == blockKey.Position) {
                // 找到该 BlockKey对应的种类 的 现存的 CompressedId，或创建一个新的
                var blockType = (blockKey.BlockTypeId, blockKey.Facing);
                if (!backwardBlockTypeLookupTable.TryGetValue(blockType, out ushort compressedId)) {
                    compressedId = incrementalCounter;
                    backwardBlockTypeLookupTable.Add(blockType, compressedId);
                    blockTypeLookupTable.Add(compressedId, blockType);
                    incrementalCounter++;
                }
                
                compressedIds.Add(compressedId);
                currentIndex++;
                if (currentIndex >= sortedBlockKeys.Count) break;
                blockKey = sortedBlockKeys[currentIndex];
            }
            
            if (compressedIds.Count == 0) return EMPTYSTR;
            
            compressedIds.Sort();
            stringBuilder.Clear();
            foreach (var compressedId in compressedIds) stringBuilder.Append((char)compressedId);
            return stringBuilder.ToString();
        }
        
        public Schematic(EverythingPool pool, Vector3Int pos1, Vector3Int pos2, ISet<BlockKey> changes, bool isSorted = true) : this(pool, pos1, pos2) {
            int xStart = pos1.x;
            int yStart = pos1.y;
            int zStart = pos1.z;
            int xEnd = pos2.x;
            int yEnd = pos2.y;
            int zEnd = pos2.z;
            if (xStart > xEnd) (xStart, xEnd) = (xEnd, xStart);
            if (yStart > yEnd) (yStart, yEnd) = (yEnd, yStart);
            if (zStart > zEnd) (zStart, zEnd) = (zEnd, zStart);
            this.pos1 = new(xStart, yStart, zStart);
            this.pos2 = new(xEnd, yEnd, zEnd);
            
            bool borrowed = false;
            List<BlockKey> sorted = null;
            try {
                if (!isSorted) {
                    sorted = pool.ActivateObject<List<BlockKey>>();
                    SortBySpaceMinToMax(changes, sorted);
                    borrowed = true;
                }
                else if (changes is ListSet<BlockKey> changesList1) 
                    sorted = changesList1.GetList();
                else throw new ArgumentException("Sorted changes input must be a ListSet<BlockKey>!");

                // 用于 CompressedId 计数自增器
                ushort incrementalCounter = 1;
                // 上一个方块坐标上的所有 CompressedId
                string lastCIds = null;
                // 目前处理到的 Enumerable 里的 index
                int enumerableIndex = 0;

                for (int x = xStart; x <= xEnd; x++)
                for (int y = yStart; y <= yEnd; y++)
                for (int z = zStart; z <= zEnd; z++) {
                    string cids = getCompressedIdsOnPosition(new Vector3Int(x, y, z), sorted, ref enumerableIndex,
                        ref incrementalCounter);
                    
                    if (lastCIds == cids) {
                        var elem = this.changes[^1];
                        this.changes[^1] = (elem.Item1, elem.Item2 + 1);
                    }
                    else {
                        this.changes.Add((cids, 1));
                        lastCIds = cids;
                    }
                }
            }
            finally {
                if (borrowed) pool.DeactivateObject(sorted);
            }
        }

        private int blockGroupIndex = 0;
        public void ResetBlockGroupIndex() => blockGroupIndex = 0;
        public int NextCompressedBlockGroup(List<(int, Facing)> result, out bool succeed) {
            result.Clear();
            succeed = true;
            if (blockGroupIndex >= changes.Count) {
                succeed = false;
                return 0;
            }
            
            var res = changes[blockGroupIndex];
            foreach (var blockTypeChar in res.Item1) {
                ushort blockType = blockTypeChar;
                if (blockType == EMPTY) continue;
                (int, Facing) blockTypeAndFacing = blockTypeLookupTable[blockType];
                result.Add(blockTypeAndFacing);
            }
            blockGroupIndex++;
            return res.Item2;
        }

        public Schematic Clone() {
            var copy = new Schematic(pool, pos1, pos2);

            foreach (var kv in blockTypeLookupTable)
                copy.blockTypeLookupTable.Add(kv.Key, kv.Value);
            foreach (var kv in backwardBlockTypeLookupTable)
                copy.backwardBlockTypeLookupTable.Add(kv.Key, kv.Value);

            if (changes.Count > 0) {
                if (copy.changes.Capacity < changes.Count)
                    copy.changes.Capacity = changes.Count;
                copy.changes.AddRange(changes);
            }
            
            copy.blockGroupIndex = this.blockGroupIndex;
            return copy;
        }

        public int GetBlockCount(bool dontCopyAir) {
            if (!dontCopyAir) 
                return BlockSelection.GetSelectionSize(pos1, pos2, out _, out _, out _);

            int count = 0;
            foreach (var (cids, count1) in changes) {
                if (cids == EMPTYSTR) continue;
                count += count1;
            }
            return count;
        }
    }
}