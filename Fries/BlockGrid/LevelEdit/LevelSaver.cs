using System.IO;
using System;
# if UNITY_EDITOR
using UnityEditor;
# endif
using UnityEngine;
using Object = UnityEngine.Object;

namespace Fries.BlockGrid.LevelEdit {
    public static class LevelSaver {
        public static void Save(GameObject gameObject, string targetFolder, string saveName, bool forceSave = false) {
            # if UNITY_EDITOR
            if (!forceSave) regularSave(gameObject, targetFolder, saveName);
            else LevelSaver.forceSave(gameObject, targetFolder, saveName);
            # endif
        }

        private static void regularSave(GameObject gameObject, string targetFolder, string saveName) {
# if UNITY_EDITOR
            LevelEditor.Inst.BlockMap.ClearAllInactives();

            TaskPerformer.TaskPerformer.inst().scheduleTask((Action)(() => {
                bool reload = false;

                // Ensure target folder exists
                if (!AssetDatabase.IsValidFolder("Assets/" + targetFolder)) {
                    Directory.CreateDirectory("Assets/" + targetFolder);
                    reload = true;
                }

                // Build paths
                string fileName = $"{saveName}-{gameObject.name}";
                string safeName = string.Join("_", fileName.Split(Path.GetInvalidFileNameChars()));
                string path = $"{targetFolder}/{safeName}.prefab";

                string safeName1 = string.Join("_", saveName.Split(Path.GetInvalidFileNameChars()));
                var dateStr = DateTime.Now.ToString(
                    "yyyyMMddHHmmssfffffff",
                    System.Globalization.CultureInfo.InvariantCulture
                );
                string path1 = $"{targetFolder}/{safeName1}/{safeName}-{dateStr}.prefab";

                // Ensure version folder exists (targetFolder/saveName)
                if (!AssetDatabase.IsValidFolder("Assets/" + $"{targetFolder}/{safeName1}")) {
                    Directory.CreateDirectory("Assets/" + $"{targetFolder}/{safeName1}");
                    reload = true;
                }

                if (reload) AssetDatabase.Refresh();

                // Save
                PrefabUtility.SaveAsPrefabAsset(gameObject, "Assets/" + path);
                PrefabUtility.SaveAsPrefabAsset(gameObject, "Assets/" + path1);

                Debug.Log($"Level has been saved at: Assets/{path}");
                EditorGUIUtility.PingObject(AssetDatabase.LoadAssetAtPath<Object>("Assets/" + path));
                EditorGUIUtility.PingObject(AssetDatabase.LoadAssetAtPath<Object>("Assets/" + path1));
            }), 1);
# endif
        }

        private static void forceSave(GameObject gameObject, string targetFolder, string saveName) {
# if UNITY_EDITOR
            bool reload = false;
            if (!AssetDatabase.IsValidFolder("Assets/" + targetFolder)) {
                Directory.CreateDirectory("Assets/" + targetFolder);
                reload = true;
            }
            
            string fileName = $"{saveName}-{gameObject.name}";
            string safeName = string.Join("_", fileName.Split(Path.GetInvalidFileNameChars()));
            string path = $"{targetFolder}/{safeName}.prefab";
    
            string safeName1 = string.Join("_", saveName.Split(Path.GetInvalidFileNameChars()));
            var dateStr = DateTime.Now.ToString("yyyyMMddHHmmssfffffff", System.Globalization.CultureInfo.InvariantCulture);
            string path1 = $"{targetFolder}/{safeName1}/{safeName}-{dateStr}.prefab";
    
            if (!AssetDatabase.IsValidFolder("Assets/" + $"{targetFolder}/{safeName1}")) {
                Directory.CreateDirectory("Assets/" + $"{targetFolder}/{safeName1}");
                reload = true;
            }
            if (reload) AssetDatabase.Refresh();
    
            PrefabUtility.SaveAsPrefabAsset(gameObject, "Assets/" + path);
            PrefabUtility.SaveAsPrefabAsset(gameObject, "Assets/" + path1);

            Debug.Log($"Level has been saved at: Assets/{path}");
            EditorGUIUtility.PingObject(AssetDatabase.LoadAssetAtPath<Object>("Assets/" + path));
            EditorGUIUtility.PingObject(AssetDatabase.LoadAssetAtPath<Object>("Assets/" + path1));
# endif
        }
    }
}