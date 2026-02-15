using System.IO;
# if UNITY_EDITOR
using System;
using UnityEditor;
# endif
using UnityEngine;
using Object = UnityEngine.Object;

namespace Fries.BlockGrid.LevelEdit {
    public static class LevelSaver {
        public static void Save(GameObject gameObject, string targetFolder, string saveName) {
            # if UNITY_EDITOR
            LevelEditor.Inst.BlockMap.ClearAllInactives();
            
            TaskPerformer.TaskPerformer.inst().scheduleTask((Action)(() => {
                if (!AssetDatabase.IsValidFolder("Assets/" + targetFolder)) {
                    Directory.CreateDirectory("Assets/" + targetFolder);
                    AssetDatabase.Refresh();
                }

                string fileName = $"{saveName}-{gameObject.name}";
                string safeName = string.Join("_", fileName.Split(Path.GetInvalidFileNameChars()));
                string path = $"{targetFolder}/{safeName}.prefab";
                PrefabUtility.SaveAsPrefabAsset(gameObject, "Assets/" + path);

                Debug.Log($"Level has been saved at: {path}");
                EditorGUIUtility.PingObject(AssetDatabase.LoadAssetAtPath<Object>(path));
            }), 1);
            # endif
        }
    }
}