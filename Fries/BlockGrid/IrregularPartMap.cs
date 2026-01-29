using System;
using System.Collections.Generic;
using Fries.Pool;
using UnityEditor;
using UnityEngine;

namespace Fries.BlockGrid {
    public class IrregularPartMap {
        public readonly float cellSize;
        private readonly EverythingPool everythingPool;

        public IrregularPartMap(float cellSize, EverythingPool everythingPool) {
            if (cellSize <= 0f) throw new ArgumentOutOfRangeException(nameof(cellSize), "cellSize must be > 0");
            this.cellSize = cellSize;
            this.everythingPool = everythingPool;
        }

        // cell -> ids in that cell
        private readonly Dictionary<Vector3Int, List<int>> spatialHash = new();
        // id -> bounds
        private readonly Dictionary<int, Bounds> boundsMap = new();
        // id -> occupied cells
        private readonly Dictionary<int, List<Vector3Int>> id2Cells = new();

        private int _nextId = 1;

        public int AddBounds(Bounds bounds) {
            int id = _nextId++;
            boundsMap[id] = bounds;

            var cells = everythingPool.ActivateObject<List<Vector3Int>>();
            getCellsCovered(bounds, cells);
            id2Cells[id] = cells;

            foreach (var cell in cells) {
                if (!spatialHash.TryGetValue(cell, out var list)) {
                    list = everythingPool.ActivateObject<List<int>>();
                    spatialHash[cell] = list;
                }
                list.Add(id);
            }
            return id;
        }

        public void RemoveBounds(int id) {
            if (!boundsMap.Remove(id)) return;

            if (!id2Cells.TryGetValue(id, out var cells)) 
                throw new KeyNotFoundException("Cannot find cells for id " + id +
                                               " when removing bounds. This is a bug and should not happen.");
            
            foreach (var cell in cells) {
                if (!spatialHash.TryGetValue(cell, out var list)) continue;
                list.Remove(id);
                if (list.Count == 0) {
                    spatialHash.Remove(cell, out var ints);
                    everythingPool.DeactivateObject(ints);
                }
            }
            id2Cells.Remove(id);
            everythingPool.DeactivateObject(cells);
        }

        public bool UpdateBounds(int id, Bounds bounds) {
            if (!boundsMap.ContainsKey(id)) return false;

            // 先从旧 cells 移除
            if (id2Cells.TryGetValue(id, out var oldCells)) {
                foreach (var cell in oldCells) {
                    if (!spatialHash.TryGetValue(cell, out var list)) continue;
                    list.Remove(id);
                    if (list.Count == 0) {
                        spatialHash.Remove(cell, out var ints);
                        everythingPool.DeactivateObject(ints);
                    }
                }
            }

            // 再添加到新 cells
            boundsMap[id] = bounds;

            var newCells = everythingPool.ActivateObject<List<Vector3Int>>();
            getCellsCovered(bounds, newCells);
            if (id2Cells.TryGetValue(id, out var oldList))
                everythingPool.DeactivateObject(oldList);
            id2Cells[id] = newCells;

            foreach (var cell in newCells) {
                if (!spatialHash.TryGetValue(cell, out var list)) {
                    list = everythingPool.ActivateObject<List<int>>();
                    spatialHash[cell] = list;
                }
                list.Add(id);
            }

            return true;
        }

        public void Clear() {
            foreach (var kv in id2Cells) 
                everythingPool.DeactivateObject(kv.Value);
            id2Cells.Clear();
            foreach (var kv in spatialHash) 
                everythingPool.DeactivateObject(kv.Value);
            spatialHash.Clear();
            boundsMap.Clear();
            _nextId = 1;
        }
        
        public bool CollisionTest(Bounds toBeTested, List<Bounds> collidesWith = null) {
            collidesWith?.Clear();

            // 候选去重：同一个 id 可能出现在多个 cell
            var visited = everythingPool.ActivateObject<HashSet<int>>();
            var cells = everythingPool.ActivateObject<List<Vector3Int>>();
            try {
                getCellsCovered(toBeTested, cells);

                foreach (var t in cells) {
                    if (!spatialHash.TryGetValue(t, out var ids)) continue;

                    foreach (var id in ids) {
                        if (!visited.Add(id)) continue;

                        if (!boundsMap.TryGetValue(id, out var b)) continue;

                        if (!b.Intersects(toBeTested)) continue;
                        if (collidesWith != null) collidesWith.Add(b);
                        else return true;
                    }
                }
            }
            finally {
                everythingPool.DeactivateObject(cells);
                everythingPool.DeactivateObject(visited);
            }

            return collidesWith is { Count: > 0 };
        }

        // ---------- helpers ----------

        private Vector3Int worldToCell(Vector3 p) {
            // Floor 对负数也正确（例如 -0.1 / 1 -> -1）
            return new Vector3Int(
                Mathf.FloorToInt(p.x / cellSize),
                Mathf.FloorToInt(p.y / cellSize),
                Mathf.FloorToInt(p.z / cellSize)
            );
        }

        private void getCellsCovered(Bounds b, List<Vector3Int> cells) {
            var min = worldToCell(b.min);
            var max = worldToCell(b.max - Vector3.one * 1e-6f);

            if (max.x < min.x) max.x = min.x;
            if (max.y < min.y) max.y = min.y;
            if (max.z < min.z) max.z = min.z;
            
            for (int x = min.x; x <= max.x; x++)
            for (int y = min.y; y <= max.y; y++)
            for (int z = min.z; z <= max.z; z++)
                cells.Add(new Vector3Int(x, y, z));
        }
        
        public void DrawAllBoundsGizmos(float lineWidth = 2f) {
#if UNITY_EDITOR
            Handles.color = Color.black;
            foreach (var b in boundsMap.Values) {
                DrawBoundsWireThick(b, lineWidth);
            }
#else
            Gizmos.color = Color.black;
            foreach (var b in boundsMap.Values) {
                Gizmos.DrawWireCube(b.center, b.size);
            }
#endif
        }

#if UNITY_EDITOR
        private static void DrawBoundsWireThick(Bounds b, float lineWidth) {
            var c = b.center;
            var e = b.extents;

            // 8 corners
            var p000 = c + new Vector3(-e.x, -e.y, -e.z);
            var p001 = c + new Vector3(-e.x, -e.y, +e.z);
            var p010 = c + new Vector3(-e.x, +e.y, -e.z);
            var p011 = c + new Vector3(-e.x, +e.y, +e.z);
            var p100 = c + new Vector3(+e.x, -e.y, -e.z);
            var p101 = c + new Vector3(+e.x, -e.y, +e.z);
            var p110 = c + new Vector3(+e.x, +e.y, -e.z);
            var p111 = c + new Vector3(+e.x, +e.y, +e.z);

            // 12 edges
            Handles.DrawAAPolyLine(lineWidth, p000, p001);
            Handles.DrawAAPolyLine(lineWidth, p000, p010);
            Handles.DrawAAPolyLine(lineWidth, p000, p100);

            Handles.DrawAAPolyLine(lineWidth, p111, p110);
            Handles.DrawAAPolyLine(lineWidth, p111, p101);
            Handles.DrawAAPolyLine(lineWidth, p111, p011);

            Handles.DrawAAPolyLine(lineWidth, p001, p011);
            Handles.DrawAAPolyLine(lineWidth, p001, p101);

            Handles.DrawAAPolyLine(lineWidth, p010, p011);
            Handles.DrawAAPolyLine(lineWidth, p010, p110);

            Handles.DrawAAPolyLine(lineWidth, p100, p101);
            Handles.DrawAAPolyLine(lineWidth, p100, p110);
        }
#endif
    }
}
