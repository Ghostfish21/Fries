using System.IO;
# if UNITY_EDITOR
using System;
using UnityEditor;
# endif
using UnityEngine;
using Object = UnityEngine.Object;

namespace Fries.BlockGrid.LevelEdit {
    public static class LevelSaver {
        public static void Save(GameObject gameObject, string targetFolder, string saveName, bool forceSave = false) {
            # if UNITY_EDITOR
            if (!forceSave) regularSave(gameObject, targetFolder, saveName);
            else fourceSave(gameObject, targetFolder, saveName);
            # endif
        }

        private static void regularSave(GameObject gameObject, string targetFolder, string saveName) {
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
        }

        private static void fourceSave(GameObject gameObject, string targetFolder, string saveName) {
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
        }
    }
}