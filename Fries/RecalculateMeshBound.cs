using UnityEditor;
using UnityEngine;

namespace Fries {
    public class RecalculateSelectedMeshesBounds {
        [MenuItem("Tools/Fries/Util/Recalculate Selected Meshes' Bound")]
        public static void RecalculateBoundsForSelectedFbx() {
            // 获取在 Project 窗口中选中的所有资源
            Object[] selectedAssets = Selection.GetFiltered(typeof(Object), SelectionMode.Assets);
            foreach (var selected in selectedAssets) {
                // 获取资源的路径
                string path = AssetDatabase.GetAssetPath(selected);

                // 判断是否是 .fbx 文件
                if (!path.ToLower().EndsWith(".fbx"))
                    continue;

                // 获取该 FBX 对应的 ModelImporter
                ModelImporter modelImporter = AssetImporter.GetAtPath(path) as ModelImporter;
                if (modelImporter == null)
                    continue;

                // 加载该 FBX 包含的所有子资源
                Object[] fbxAssets = AssetDatabase.LoadAllAssetsAtPath(path);

                bool hasMeshChanged = false;
                foreach (Object asset in fbxAssets) {
                    if (asset is Mesh mesh) {
                        // 重新计算 Mesh 的 Bounds
                        mesh.RecalculateBounds();

                        hasMeshChanged = true;
                        EditorUtility.SetDirty(mesh);
                        EditorUtility.SetDirty(asset);
                    }
                }

                // 如果有 Mesh 发生变化，尝试保存
                if (hasMeshChanged) {
                    AssetDatabase.SaveAssets();
                }
            }

            // 最后刷新一下 AssetDatabase
            AssetDatabase.Refresh();
        }
    }
}

