namespace Fries.BlockGrid {
    using System.Collections.Generic;
    using UnityEngine;

#if UNITY_EDITOR
    using UnityEditor;
#endif

    namespace Fries.BlockGrid {
        internal static class BlockGridGizmos {
            // 避免 Scene 视图卡死：体素总量过大时会自动降级绘制
            private const int MaxCellsToDraw = 4096;

#if UNITY_EDITOR
            public static void Draw(Transform root, float unitLength,
                IReadOnlyDictionary<Vector3Int, HashSet<int>> blockMap, int gridLength = 1) {
                if (!root) return;
                if (unitLength <= 0f) unitLength = 1f;

                Transform active = Selection.activeTransform;
                if (!active) return;

                bool selectedSelf = active == root;
                bool selectedChild = !selectedSelf && active.IsChildOf(root);
                if (!selectedSelf && !selectedChild) return;

                // 颜色：选中本体白线；选中子物体灰线（都“淡淡的”）
                Color lineBase = selectedSelf ? Color.white : Color.gray;
                float alpha = selectedSelf ? 0.35f : 0.25f;
                Color lineColor = new Color(lineBase.r, lineBase.g, lineBase.b, alpha);

                // 计算需要绘制的体素范围（没有任何方块时，把 (0,0,0) 当作有方块）
                Vector3Int min, max;
                GetBounds(blockMap, out min, out max);

                int sizeX = max.x - min.x + 1;
                int sizeY = max.y - min.y + 1;
                int sizeZ = max.z - min.z + 1;
                long cellCount = (long)sizeX * sizeY * sizeZ;

                // 在 root 的本地空间画 Gizmos（跟随 root 的旋转/缩放/位移）
                Matrix4x4 oldMatrix = Gizmos.matrix;
                Color oldColor = Gizmos.color;
                Gizmos.matrix = root.localToWorldMatrix;
                Gizmos.color = lineColor;

                // 体素线框：默认绘制包围盒内全部体素；太大时降级为只画“已有方块/或原点”体素
                if (cellCount > MaxCellsToDraw) {
                    if (blockMap is { Count: > 0 }) {
                        foreach (var kv in blockMap) DrawVoxelWireCube(kv.Key, unitLength);
                    }
                    else DrawVoxelWireCube(Vector3Int.zero, unitLength);
                }
                else {
                    for (int x = min.x; x <= max.x; x++) {
                        for (int y = min.y; y <= max.y; y++) {
                            for (int z = min.z; z <= max.z; z++) {
                                DrawVoxelWireCube(new Vector3Int(x, y, z), unitLength);
                            }
                        }
                    }
                }

                Gizmos.matrix = oldMatrix;
                Gizmos.color = oldColor;

                // 选中子物体时：在子物体位置写网格坐标
                if (selectedChild) {
                    Vector3 local = root.InverseTransformPoint(active.position);
                    Vector3 scaled = local / unitLength;

                    // 你的 block 放置是 x*GridLength 这种离散点位，round 最贴近你的用法
                    Vector3Int gridPos = new Vector3Int(
                        Mathf.RoundToInt(scaled.x),
                        Mathf.RoundToInt(scaled.y),
                        Mathf.RoundToInt(scaled.z)
                    );

                    var style = new GUIStyle(EditorStyles.boldLabel);
                    style.fontSize = 11;
                    style.normal.textColor = new Color(lineBase.r, lineBase.g, lineBase.b, 0.9f);

                    Handles.Label(active.position, $"({gridPos.x}, {gridPos.y}, {gridPos.z})", style);
                }
            }

            private static void GetBounds(IReadOnlyDictionary<Vector3Int, HashSet<int>> blockMap, out Vector3Int min,
                out Vector3Int max) {
                if (blockMap == null || blockMap.Count == 0) {
                    min = max = Vector3Int.zero;
                    return;
                }

                bool inited = false;
                min = max = Vector3Int.zero;

                foreach (var kv in blockMap) {
                    Vector3Int p = kv.Key;
                    if (!inited) {
                        min = max = p;
                        inited = true;
                    }
                    else {
                        min = Vector3Int.Min(min, p);
                        max = Vector3Int.Max(max, p);
                    }
                }
            }

            private static void DrawVoxelWireCube(Vector3Int gridPos, float gridLength) {
                Vector3 center = new Vector3(gridPos.x * gridLength, gridPos.y * gridLength, gridPos.z * gridLength);
                Gizmos.DrawWireCube(center, Vector3.one * gridLength);
            }
#endif
        }
    }

}