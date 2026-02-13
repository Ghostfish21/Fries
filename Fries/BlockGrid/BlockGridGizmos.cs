using System.Text;
using Fries.Data;
using Fries.Pool;

namespace Fries.BlockGrid {
    using System.Collections.Generic;
    using UnityEngine;

#if UNITY_EDITOR
    using UnityEditor;
#endif

    namespace Fries.BlockGrid {
        internal static class BlockGridGizmos {
#if UNITY_EDITOR
            private const int Radius = 2; // 向外扩张 2 格（欧氏距离）
            private const float EdgeAlphaFactor = 0.15f; // 外圈透明度系数（越外越淡，但不至于 0）
            private const float MinAlphaToDraw = 0.01f; // 太淡就不画（省性能/避免闪）

            public static void Draw(
                Transform root,
                float unitLength,
                IReadOnlyDictionary<Vector3Int, Dictionary<int, ListSet<Facing>>> blockMap,
                IReadOnlyDictionary<BlockKey, Dictionary<int, object>> blockDataDict,
                IReadOnlyDictionary<GameObject, BlockKey> instance2Key,
                IReadOnlyDictionary<BlockKey, Color> dyedBlocks,
                EverythingPool everythingPool
            ) {
                if (!root) return;
                if (unitLength <= 0f) unitLength = 1f;
                if (blockMap == null || blockMap.Count == 0) return;

                Transform active = Selection.activeTransform;
                if (!active) return;

                for (int i = 0; i < 100; i++) {
                    if (!active.IsChildOf(root)) {
                        if (!active.parent) return;
                        active = active.parent;
                    }
                    else break;
                }

                bool selectedSelf = active == root;
                bool selectedChild = !selectedSelf && active.IsChildOf(root);
                if (!selectedSelf && !selectedChild) return;

                // 颜色基调：选中本体更亮；选中子物体偏灰
                Color lineBase = selectedSelf ? Color.white : Color.gray;
                float baseAlpha = selectedSelf ? 0.35f : 0.25f;

                // -----------------------------
                // 1) 计算 origin（网格坐标）
                // -----------------------------
                Vector3Int origin;
                BlockKey? blockKey = null;
                if (selectedSelf) {
                    // origin = blockMap.position -> 对应 root 本地空间 (0,0,0)
                    origin = Vector3Int.zero;
                }
                else {
                    // origin = blockMap 里找到 child 的 BlockKey.position
                    if (instance2Key == null) return;
                    if (!instance2Key.TryGetValue(active.gameObject, out var key)) return;
                    origin = key.Position;
                    blockKey = key;
                }

                // -----------------------------
                // 2) 在 root 的本地空间画 Gizmos
                // -----------------------------
                Matrix4x4 oldMatrix = Gizmos.matrix;
                Color oldColor = Gizmos.color;
                Gizmos.matrix = root.localToWorldMatrix;

                int r = Radius;
                int r2 = r * r;

                // 只检查 origin 周围 5*5*5 = 125 个格子（再用欧氏距离过滤）
                for (int dx = -r; dx <= r; dx++)
                for (int dy = -r; dy <= r; dy++)
                for (int dz = -r; dz <= r; dz++) {
                    int ds = dx * dx + dy * dy + dz * dz;
                    if (ds > r2) continue; // 欧氏距离过滤

                    Vector3Int p = new Vector3Int(origin.x + dx, origin.y + dy, origin.z + dz);

                    // 只画“存在方块”的格子
                    if (!blockMap.TryGetValue(p, out var ids) || ids == null || ids.Count == 0)
                        continue;

                    float dist = Mathf.Sqrt(ds); // 0..Radius
                    float t = dist / r; // 0..1
                    float alphaFactor = Mathf.Lerp(1f, EdgeAlphaFactor, t);

                    float a = baseAlpha * alphaFactor;
                    if (a < MinAlphaToDraw) continue;

                    Gizmos.color = new Color(lineBase.r, lineBase.g, lineBase.b, a);
                    DrawVoxelWireCube(p, unitLength);
                }


                Gizmos.matrix = oldMatrix;
                Gizmos.color = oldColor;

                // -----------------------------
                // 3) 选中子物体时：显示 origin 的格子坐标（就是 BlockKey.position）
                // -----------------------------
                if (selectedChild) 
                    DrawInfo(lineBase, active.position, origin, blockKey, blockDataDict, everythingPool);

                foreach (var dyedBlock in dyedBlocks) {
                    Gizmos.color = new Color(dyedBlock.Value.r, dyedBlock.Value.g, dyedBlock.Value.b, 1);
                    DrawVoxelWireCube(dyedBlock.Key.Position, unitLength);
                    DrawInfo(dyedBlock.Value, dyedBlock.Key.Position, dyedBlock.Key.Position, dyedBlock.Key, blockDataDict, everythingPool);
                }
            }

            private static void DrawInfo(Color lineBase, Vector3 position, Vector3Int origin, BlockKey? blockKey, IReadOnlyDictionary<BlockKey, Dictionary<int, object>> blockDataDict, EverythingPool everythingPool) {
                var style = new GUIStyle(EditorStyles.boldLabel) {
                    fontSize = 11
                };
                style.normal.textColor = new Color(lineBase.r, lineBase.g, lineBase.b, 0.9f);

                Handles.Label(position,
                    $"({origin.x}, {origin.y}, {origin.z}) [t:{blockKey.Value.BlockTypeId}]", style);
                if (blockDataDict.TryGetValue(blockKey.Value, out var dataDict)) {
                    StringBuilder dataStr = everythingPool.ActivateObject<StringBuilder>();
                    int lastNewLine = 0;
                    bool nl = false;
                    foreach (var (key, value) in dataDict) {
                        nl = false;
                        dataStr.Append($"[{key}]={value}, ");
                        if (dataStr.Length - lastNewLine < 40) continue;
                        dataStr.Append('\n');
                        lastNewLine = dataStr.Length;
                        nl = true;
                    }

                    if (dataStr.Length >= 2)
                        dataStr.Length -= 2;
                    if (nl) dataStr.Length--;

                    Handles.Label(position + Vector3.up * 1.5f, dataStr.ToString(), style);
                    everythingPool.DeactivateObject(dataStr);
                }
            }

            private static void DrawVoxelWireCube(Vector3Int gridPos, float unitLength) {
                Vector3 center = new Vector3(gridPos.x * unitLength, gridPos.y * unitLength, gridPos.z * unitLength);
                Gizmos.DrawWireCube(center, Vector3.one * unitLength);
            }
#endif
        }
    }
}
