#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Fries.BlockGrid.Fries.BlockGrid {
    internal static class BlockGridGizmos {
        private const int Radius = 4;               // 向外扩张 4 格
        private const int MaxCellsToDraw = 4096;    // 保险丝

        public static void Draw(
            Transform root,
            float unitLength,
            IReadOnlyDictionary<Vector3Int, HashSet<int>> blockMap,
            IReadOnlyDictionary<int, Dictionary<Vector3Int, GameObject>> blockInstances
        ) {
            if (!root) return;
            if (unitLength <= 0f) unitLength = 1f;

            Transform active = Selection.activeTransform;
            if (!active) return;

            bool selectedSelf = active == root;
            bool selectedChild = !selectedSelf && active.IsChildOf(root);
            if (!selectedSelf && !selectedChild) return;

            // 颜色：选中本体白线；选中子物体灰线
            Color lineBase = selectedSelf ? Color.white : Color.gray;
            float alpha = selectedSelf ? 0.35f : 0.25f;
            Color lineColor = new Color(lineBase.r, lineBase.g, lineBase.b, alpha);

            // origin 网格坐标：
            // - 选中本体：origin = blockMap.position => 网格(0,0,0)
            // - 选中子物体：origin = 在 blockMap/instances 中找到该 child 对应的格子
            Vector3Int originGridPos = Vector3Int.zero;
            if (selectedChild) {
                if (!TryGetOriginFromSelectedChild(root, unitLength, active, blockMap, blockInstances, out originGridPos)) {
                    // 找不到就退化为“按位置 round”，至少能用
                    originGridPos = ApproxGridPos(root, unitLength, active.position);
                }
            }

            // 在 root 的本地空间画 Gizmos（跟随 root 的旋转/缩放/位移）
            Matrix4x4 oldMatrix = Gizmos.matrix;
            Color oldColor = Gizmos.color;
            Gizmos.matrix = root.localToWorldMatrix;

            int drawn = 0;

            // 只扫描 origin 周围 4 格范围内的格子，并且只画“确实存在方块”的格子
            for (int dx = -Radius; dx <= Radius; dx++) {
                for (int dy = -Radius; dy <= Radius; dy++) {
                    for (int dz = -Radius; dz <= Radius; dz++) {
                        Vector3Int p = originGridPos + new Vector3Int(dx, dy, dz);

                        if (!blockMap.TryGetValue(p, out var ids) || ids == null || ids.Count == 0)
                            continue;

                        Gizmos.color = lineColor;
                        DrawVoxelWireCube(p, unitLength);

                        if (++drawn >= MaxCellsToDraw) goto END_DRAW;
                    }
                }
            }

        END_DRAW:
            Gizmos.matrix = oldMatrix;
            Gizmos.color = oldColor;

            //（可选）选中子物体时显示它的格子坐标
            if (selectedChild) {
                var style = new GUIStyle(EditorStyles.boldLabel) {
                    fontSize = 11,
                    normal = { textColor = new Color(lineBase.r, lineBase.g, lineBase.b, 0.9f) }
                };
                Handles.Label(active.position, $"({originGridPos.x}, {originGridPos.y}, {originGridPos.z})", style);
            }
        }

        private static bool TryGetOriginFromSelectedChild(
            Transform root,
            float unitLength,
            Transform active,
            IReadOnlyDictionary<Vector3Int, HashSet<int>> blockMap,
            IReadOnlyDictionary<int, Dictionary<Vector3Int, GameObject>> blockInstances,
            out Vector3Int origin
        ) {
            origin = default;

            if (blockMap == null || blockMap.Count == 0 || blockInstances == null || blockInstances.Count == 0)
                return false;

            // 先用“近邻格子”快速命中（避免每帧全表扫描）
            Vector3Int approx = ApproxGridPos(root, unitLength, active.position);

            for (int dx = -1; dx <= 1; dx++) {
                for (int dy = -1; dy <= 1; dy++) {
                    for (int dz = -1; dz <= 1; dz++) {
                        Vector3Int p = approx + new Vector3Int(dx, dy, dz);

                        if (!blockMap.TryGetValue(p, out var ids) || ids == null || ids.Count == 0)
                            continue;

                        foreach (int id in ids) {
                            if (!blockInstances.TryGetValue(id, out var instMap) || instMap == null) continue;
                            if (!instMap.TryGetValue(p, out var go) || !go) continue;

                            Transform t = go.transform;
                            if (active == t || active.IsChildOf(t)) {
                                origin = p; // ✅ 这就是 blockMap 里的 BlockKey.position
                                return true;
                            }
                        }
                    }
                }
            }

            // 退化：万一 prefab.localPosition 之类导致近邻没命中，才做全表扫描兜底
            foreach (var idKvp in blockInstances) {
                var instMap = idKvp.Value;
                if (instMap == null) continue;

                foreach (var kvp in instMap) {
                    var p = kvp.Key;
                    var go = kvp.Value;
                    if (!go) continue;

                    Transform t = go.transform;
                    if (active == t || active.IsChildOf(t)) {
                        origin = p;
                        return true;
                    }
                }
            }

            return false;
        }

        private static Vector3Int ApproxGridPos(Transform root, float unitLength, Vector3 worldPos) {
            Vector3 local = root.InverseTransformPoint(worldPos);
            Vector3 scaled = local / unitLength;
            return new Vector3Int(
                Mathf.RoundToInt(scaled.x),
                Mathf.RoundToInt(scaled.y),
                Mathf.RoundToInt(scaled.z)
            );
        }

        private static void DrawVoxelWireCube(Vector3Int gridPos, float unitLength) {
            Vector3 center = new Vector3(gridPos.x * unitLength, gridPos.y * unitLength, gridPos.z * unitLength);
            Gizmos.DrawWireCube(center, Vector3.one * unitLength);
        }
    }
}
#endif
