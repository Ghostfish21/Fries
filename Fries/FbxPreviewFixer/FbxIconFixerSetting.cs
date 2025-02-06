# if UNITY_EDITOR

using UnityEngine;
using UnityEditor;
using System.Reflection;
using System.IO;
using UnityEngine.SceneManagement;
using UnityEditor.SceneManagement;
using System.Threading.Tasks;

namespace Fries.FbxPreviewFixer {

    public class FbxIconFixerWindow : EditorWindow {
        // 对应我们在 FbxIconFixer 中使用的 key
        private string SettingKey = $"Fbx_Icon_Fixer.{SystemUtils.projectName()}.Fbx_Icon_Path";
        private string SettingKeyIsEnabled = $"Fbx_Icon_Fixer.{SystemUtils.projectName()}.Is_Enabled";
        private string SettingKeyScenePath = $"Fbx_Icon_Fixer.{SystemUtils.projectName()}.Scene_Path";
        private string SettingKeyResolution = $"Fbx_Icon_Fixer.{SystemUtils.projectName()}.Resolution";

        private string _scenePath;

        // 用于在窗口中显示和编辑的本地字段
        private string _fbxIconPath;
        private bool _isEnabled;

        private int _resolution;

        // 添加菜单项，点击后打开该窗口
        [MenuItem("Tools/Fries/Util/Fbx Icon Fixer Settings")]
        private static void ShowWindow() {
            // 创建并显示窗口
            var window = GetWindow<FbxIconFixerWindow>("Fbx Icon Fixer Settings");
            window.Show();
        }

        // 窗口启用时，从 EditorPrefs 读取当前存储的值
        private void OnEnable() {
            // 如果没存过值，默认给一个 "Assets/FbxIconPath"
            // ProjectWindowIconDrawer.setup();
            _fbxIconPath = EditorPrefs.GetString(SettingKey, "Assets/Editor/Fbx Icons");
            _scenePath = EditorPrefs.GetString(SettingKeyScenePath, "");
            _isEnabled = EditorPrefs.GetBool(SettingKeyIsEnabled, true);
            _resolution = EditorPrefs.GetInt(SettingKeyResolution, 256);
        }


        // 渲染窗口界面
        private void OnGUI() {
            EditorGUILayout.LabelField("Is Enabled", EditorStyles.boldLabel);
            _isEnabled = EditorGUILayout.Toggle("Enabled", _isEnabled);
            
            EditorGUILayout.LabelField("Scene Path", EditorStyles.boldLabel);
            _scenePath = EditorGUILayout.TextField("Scene Path", _scenePath);

            EditorGUILayout.LabelField("FBX Icon Path", EditorStyles.boldLabel);
            _fbxIconPath = EditorGUILayout.TextField("Path", _fbxIconPath);
            
            EditorGUILayout.LabelField("Screenshot Resolution", EditorStyles.boldLabel);
            _resolution = EditorGUILayout.IntField("Screenshot Resolution", _resolution);

            // 点击“Save”按钮后，将新的路径写入 EditorPrefs
            if (GUILayout.Button("Save")) {
                EditorPrefs.SetString(SettingKey, _fbxIconPath);
                EditorPrefs.SetBool(SettingKeyIsEnabled, _isEnabled);
                EditorPrefs.SetString(SettingKeyScenePath, _scenePath);
                EditorPrefs.SetInt(SettingKeyResolution, _resolution);
                Debug.Log("Settings Saved!");
            }

            if (GUILayout.Button("Fix Selected FBX")) {
                FixFbxIcons();
            }
        }

        private async void FixFbxIcons() {
            // 获取选中的 FBX 资源
            string[] selectedGuids = Selection.assetGUIDs;

            // 获得当前场景
            Scene currentScene = EditorSceneManager.GetActiveScene();
            string currentScenePath = currentScene.path;
            string newScenePath;

            // 创建一个新的场景
            if (_scenePath == "") {
                newScenePath = "Assets/Fbx Icon Fixer.unity";
                Scene newScene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);
                newScene.name = "Fbx Icon Fixer";
                // 保存场景到 Asset/Fbx Icon Fixer.unity
                EditorSceneManager.SaveScene(newScene, newScenePath);
            }
            else {
                if (!_scenePath.EndsWith(".unity"))
                    newScenePath = _scenePath + ".unity";
                else newScenePath = _scenePath;
            }

            // 加载场景
            EditorSceneManager.OpenScene(newScenePath);

            // 设置 SceneView 的参数
            SceneView sceneView = GetWindow<SceneView>(true, "Scene", true);
            if (sceneView != null) {
                // 设置字段视角等参数
                sceneView.cameraSettings.fieldOfView = 4f;

                // 可以手动设置一下 SceneView 窗口大小
                var pos = sceneView.position;
                pos.width = _resolution;
                pos.height = _resolution;
                sceneView.position = pos;
                
                // 如果想让它显示在前台
                sceneView.Focus();
            }
            else
                Debug.LogError("Failed to get or create SceneView.");

            await Task.Delay(1000);

            // 遍历选中的 FBX 资源
            foreach (var guid in selectedGuids) {
                string assetPath = AssetDatabase.GUIDToAssetPath(guid);
                // 只处理 FBX 文件
                if (Path.GetExtension(assetPath).ToLower() == ".fbx" || Path.GetExtension(assetPath).ToLower() == ".prefab") {
                    // 加载模型
                    GameObject modelPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(assetPath);
                    if (modelPrefab == null) {
                        Debug.LogWarning($"Asset at {assetPath} could not be loaded");
                        continue;
                    }
                    GameObject instance = PrefabUtility.InstantiatePrefab(modelPrefab) as GameObject;
                    if (instance == null) {
                        Debug.LogWarning($"Asset at {assetPath} could not be instantiated");
                        continue;
                    }

                    // 选中它并聚焦
                    Selection.activeGameObject = instance;
                    if (sceneView != null) {
                        sceneView.FrameSelected();
                        sceneView.Repaint();
                        await Task.Delay(700);
                    } else {
                        Debug.LogError("Failed to get or create SceneView.");
                    }

                    // 截图
                    string iconPath = _fbxIconPath;
                    Texture2D screenshot = CaptureSceneView(sceneView, _resolution, _resolution);
                    if (screenshot != null) {
                        // 如果设置的目录不存在，则创建
                        if (!Directory.Exists(iconPath))
                            Directory.CreateDirectory(iconPath);
                        // 保存PNG
                        string pngFullPath = Path.Combine(iconPath, guid + ".png");
                        File.WriteAllBytes(pngFullPath, screenshot.EncodeToPNG());
                        AssetDatabase.Refresh();

                        // 将生成的PNG设置为FBX icon
                        Texture2D loadedIcon = AssetDatabase.LoadAssetAtPath<Texture2D>(pngFullPath);
                    }

                    // 销毁临时实例
                    Object.DestroyImmediate(instance);
                }
            }

            // 恢复当前场景
            EditorSceneManager.OpenScene(currentScenePath);
            // 删除新场景
            if (_scenePath == "") 
                AssetDatabase.DeleteAsset(newScenePath);
            // 刷新
            AssetDatabase.Refresh();
            ProjectWindowIconDrawer.setup();

        }

        private static Texture2D CaptureSceneView(SceneView view, int width, int height) {
            if (view == null || view.camera == null) 
                return null;

            // 让场景重新绘制
            view.Repaint();
            // 等待一帧很常见，可以考虑 EditorApplication.delayCall 或更复杂的机制保证画面已更新
            
            // 临时 RenderTexture
            RenderTexture tempRT = new RenderTexture(width, height, 24);
            Camera cam = view.camera;
            var backupRT = RenderTexture.active;

            try {
                cam.targetTexture = tempRT;
                cam.Render();

                RenderTexture.active = tempRT;
                Texture2D tex = new Texture2D(width, height, TextureFormat.RGBA32, false);
                tex.ReadPixels(new Rect(0, 0, width, height), 0, 0);
                tex.Apply();
                return tex;
            }
            finally {
                cam.targetTexture = null;
                RenderTexture.active = backupRT;
                Object.DestroyImmediate(tempRT);
            }
        }

        private static void SetIconForObject(Object obj, Texture2D icon) {
            // EditorGUIUtility.SetIconForObject(obj, icon);
            // // 刷新
            // AssetDatabase.Refresh();
            MethodInfo setIconMethod = typeof(EditorGUIUtility).GetMethod(
                "SetIconForObject",
                BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public
            );
            if (setIconMethod != null)
                setIconMethod.Invoke(null, new object[] { obj, icon });
            else
                Debug.LogWarning("Could not find SetIconForObject method");
        }
    }


}

# endif