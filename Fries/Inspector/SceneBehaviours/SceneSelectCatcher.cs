

namespace Fries.Inspector.SceneBehaviours {
    # if UNITY_EDITOR
    using System;
    using System.Collections.Generic;
    using System.Reflection;
    using UnityEditor;
    using UnityEditor.SceneManagement;
    using UnityEngine;
    using UnityEngine.SceneManagement;

    [InitializeOnLoad]
    public static class SceneSelectCatcher {
        public static Func<int, Scene> getSceneByHandle;
        private static EditorWindow defaultPB;
        private static EditorWindow fakePB;
        private static Dictionary<string, SceneSelectionProxy> proxies;

        public static void registerProxy(string sceneName, SceneSelectionProxy value) {
            proxies[sceneName] = value;
        }
        public const string resourceFolder = "Scene Data";
        public const string resourcePath = "Assets/Resources/";

        private static string lastProjectWindowPath;
        
        static SceneSelectCatcher() {
            // var[] pbs = EditorUtility.Get
            
            if (!AssetDatabase.IsValidFolder(resourcePath+resourceFolder)) {
                AssetDatabase.CreateFolder(resourcePath.Substring(0, resourcePath.Length - 1), resourceFolder);
                AssetDatabase.Refresh();
            }
            proxies = new Dictionary<string, SceneSelectionProxy>();
            var loaded = Resources.LoadAll<SceneSelectionProxy>(resourcePath + resourceFolder);
            foreach (var p in loaded) 
                if (!string.IsNullOrEmpty(p.sceneName)) proxies[p.sceneName] = p;

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
            if (!proxies.TryGetValue(name, out var proxy)) {
                proxy = ScriptableObject.CreateInstance<SceneSelectionProxy>();
                proxy.sceneHandle = handle;
                proxy.scenePath = scene.path;
                proxy.sceneName = name;
                // 保存资产到 Resources 文件夹
                string assetPath = $"{resourcePath}{resourceFolder}/{name}.asset";
                AssetDatabase.CreateAsset(proxy, assetPath);
                AssetDatabase.SaveAssets();
                proxies[name] = proxy;
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