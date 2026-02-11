using System.IO;
# if UNITY_EDITOR
using UnityEditor;
# endif
using UnityEngine;

namespace Fries.BlockGrid.LevelEdit {
    public static class LevelSaver {
        public static void Save(GameObject gameObject, string targetFolder, string saveName) {
            # if UNITY_EDITOR
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
            # endif
        }
    }
}