

namespace Fries.Inspector.SceneBehaviours {
    

    using System;
    using System.Collections.Generic;
    using System.Reflection;
    using UnityEngine;
    using UnityEngine.UI;
    
# if UNITY_EDITOR
    using UnityEditor;
    using UnityEditor.SceneManagement;
    using UnityEngine.SceneManagement;
# endif
    public static class SceneBehaviourData {
        public static Dictionary<string, SceneSelectionProxy> proxies;
        public const string resourceFolder = "Scene Data";
        public const string resourcePath = "Assets/Resources/";

        public static void registerProxy(string sceneName, SceneSelectionProxy value) {
            proxies[sceneName] = value;
        }
    }
    
    # if UNITY_EDITOR
    [InitializeOnLoad]
    public static class SceneSelectCatcher {
        public static Func<int, Scene> getSceneByHandle;
        private static EditorWindow defaultPB;
        private static EditorWindow fakePB;

        private static string lastProjectWindowPath;
        
        static SceneSelectCatcher() {
            // var[] pbs = EditorUtility.Get
            
            if (!AssetDatabase.IsValidFolder(SceneBehaviourData.resourcePath+SceneBehaviourData.resourceFolder)) {
                AssetDatabase.CreateFolder(SceneBehaviourData.resourcePath.Substring(0, SceneBehaviourData.resourcePath.Length - 1), SceneBehaviourData.resourceFolder);
                AssetDatabase.Refresh();
            }
            SceneBehaviourData.proxies = new Dictionary<string, SceneSelectionProxy>();
            var loaded = Resources.LoadAll<SceneSelectionProxy>(SceneBehaviourData.resourcePath + SceneBehaviourData.resourceFolder);
            foreach (var p in loaded) 
                if (!string.IsNullOrEmpty(p.sceneName)) SceneBehaviourData.proxies[p.sceneName] = p;

            Selection.selectionChanged += OnSelectionChanged;
            
            MethodInfo mi = typeof(EditorSceneManager)
                .GetMethod("GetSceneByHandle", BindingFlags.NonPublic | BindingFlags.Static, null, new Type[] { typeof(int) }, null);
            
            if (mi == null) return;
            getSceneByHandle = (Func<int, Scene>)Delegate.CreateDelegate(typeof(Func<int, Scene>), mi);
        }

        private static void OnSelectionChanged() {
            if (Selection.instanceIDs.Nullable().Length == 0) {
                onDeselectScene();
                return;
            }
            Scene scene = getSceneByHandle(Selection.instanceIDs[0]);
            if (scene.IsValid()) 
                onSelectScene(scene, Selection.instanceIDs[0]);
            else onDeselectScene();
        }

        private static void onSelectScene(Scene scene, int handle) {
            var name = scene.path.Replace('/', '\u00a6').Replace('\\', '\u00a6');
            // 获取或创建对应的 proxy
            if (!SceneBehaviourData.proxies.TryGetValue(name, out var proxy)) {
                proxy = ScriptableObject.CreateInstance<SceneSelectionProxy>();
                proxy.sceneHandle = handle;
                proxy.scenePath = scene.path;
                proxy.sceneName = name;
                // 保存资产到 Resources 文件夹
                string assetPath = $"{SceneBehaviourData.resourcePath}{SceneBehaviourData.resourceFolder}/{name}.asset";
                AssetDatabase.CreateAsset(proxy, assetPath);
                AssetDatabase.SaveAssets();
                SceneBehaviourData.proxies[name] = proxy;
            }
            else {
                // 更新 handle 和 path
                proxy.sceneHandle = handle;
                proxy.scenePath = scene.path;
            }

            FakeHighlightRenderer.currentInstanceId = proxy.sceneHandle;

            lastProjectWindowPath = "";
            EditorApplication.delayCall += () => {
                InspectorUtil.showInInspector(proxy);
            };
        }

        private static void onDeselectScene() {
            if (lastProjectWindowPath != null) {
                lastProjectWindowPath = null;
                FakeHighlightRenderer.clear();
                InspectorUtil.cancelLock();
            }
        }
    }
    # endif
}