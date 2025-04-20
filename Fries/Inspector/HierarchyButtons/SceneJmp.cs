using System;
# if UNITY_EDITOR
using UnityEditor;
# endif
using UnityEngine;
using Object = UnityEngine.Object;

namespace Fries.Inspector.HierarchyButtons {
    public class SceneJmp : MonoBehaviour {
        private void Awake() {
            Destroy(this);
        }
        
        [SerializeField] private Object sceneAsset;
        [SerializeField] private string sceneName;

        private void OnValidate() {
            if (sceneAsset != null) {
                sceneName = sceneAsset.name;
            }
        }

        public void jmp(Event evt) {
# if UNITY_EDITOR
            if (sceneAsset == null) return;
            string scenePath = AssetDatabase.GetAssetPath(sceneAsset);
            if (string.IsNullOrEmpty(scenePath)) return;
            // 检查当前场景是否已保存
            if (UnityEditor.SceneManagement.EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) 
                UnityEditor.SceneManagement.EditorSceneManager.OpenScene(scenePath);
# endif
        }
    }
}