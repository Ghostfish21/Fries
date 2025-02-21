# if UNITY_EDITOR

using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;
using UnityEngine.SceneManagement;
using UnityEditor.SceneManagement;
using System.Threading.Tasks;

namespace Fries.FbxFunctions.FbxSnapshots {

    public class FbxSnapshot : EditorWindow {
        // 对应我们在 FbxIconFixer 中使用的 key
        private string SettingKey = $"Fbx_Snapshot.{SystemUtils.projectName()}.Output_Path";
        private string SettingKeyScenePath = $"Fbx_Snapshot.{SystemUtils.projectName()}.Scene_Path";
        private string SettingKeyResolution = $"Fbx_Snapshot.{SystemUtils.projectName()}.Resolution";
        private string SettingKeyInspectMaterial = $"Fbx_Snapshot.{SystemUtils.projectName()}.Inspect_Material";
        private string SettingKeyWhiteMaterial = $"Fbx_Snapshot.{SystemUtils.projectName()}.White_Mat";
        private string SettingKeyYellowMaterial = $"Fbx_Snapshot.{SystemUtils.projectName()}.Yellow_Mat";

        private string _scenePath;

        // 用于在窗口中显示和编辑的本地字段
        private string _fbxIconPath;
        private int _resolution;
        private bool _inspectMaterial;
        [SerializeField] private Material _yellowMat;
        [SerializeField] private Material _whiteMat;

        private bool isStarted = false;
        private bool stopFlag = false;

        // 添加菜单项，点击后打开该窗口
        [MenuItem("Tools/Fries/Fbx/Snapshots")]
        private static void ShowWindow() {
            // 创建并显示窗口
            var window = GetWindow<FbxSnapshot>("Fbx Snapshots");
            window.Show();
        }

        // 窗口启用时，从 EditorPrefs 读取当前存储的值
        private void OnEnable() {
            // 如果没存过值，默认给一个 "Assets/FbxIconPath"
            _fbxIconPath = EditorPrefs.GetString(SettingKey, "Assets/Editor/Fbx Snapshots");
            _scenePath = EditorPrefs.GetString(SettingKeyScenePath, "");
            _resolution = EditorPrefs.GetInt(SettingKeyResolution, 256);
            _inspectMaterial = EditorPrefs.GetBool(SettingKeyInspectMaterial, false);
            
            string yellowMatPath = EditorPrefs.GetString(SettingKeyYellowMaterial, "");
            if (!string.IsNullOrEmpty(yellowMatPath))
                _yellowMat = AssetDatabase.LoadAssetAtPath<Material>(yellowMatPath);
            string whiteMatPath = EditorPrefs.GetString(SettingKeyWhiteMaterial, "");
            if (!string.IsNullOrEmpty(whiteMatPath))
                _whiteMat = AssetDatabase.LoadAssetAtPath<Material>(whiteMatPath);
        }


        // 渲染窗口界面
        private void OnGUI() {
            EditorGUILayout.LabelField("Scene Path", EditorStyles.boldLabel);
            _scenePath = EditorGUILayout.TextField("Path", _scenePath);

            EditorGUILayout.LabelField("FBX Icon Path", EditorStyles.boldLabel);
            _fbxIconPath = EditorGUILayout.TextField("Path", _fbxIconPath);

            EditorGUILayout.LabelField("Screenshot Resolution", EditorStyles.boldLabel);
            _resolution = EditorGUILayout.IntField("Resolution", _resolution);
            
            EditorGUILayout.LabelField("Inspect Material", EditorStyles.boldLabel);
            _inspectMaterial = EditorGUILayout.Toggle("Enabled", _inspectMaterial);
            _yellowMat = EditorGUILayout.ObjectField("Yellow Material", _yellowMat, typeof(Material), false) as Material;
            _whiteMat = EditorGUILayout.ObjectField("White Material", _whiteMat, typeof(Material), false) as Material;
            
            // 点击“Save”按钮后，将新的路径写入 EditorPrefs
            if (GUILayout.Button("Save")) {
                EditorPrefs.SetString(SettingKey, _fbxIconPath);
                EditorPrefs.SetString(SettingKeyScenePath, _scenePath);
                EditorPrefs.SetInt(SettingKeyResolution, _resolution);
                EditorPrefs.SetBool(SettingKeyInspectMaterial, _inspectMaterial);
                
                if (_yellowMat != null) {
                    string yellowMatPath = AssetDatabase.GetAssetPath(_yellowMat);
                    EditorPrefs.SetString(SettingKeyYellowMaterial, yellowMatPath);
                }
                if (_whiteMat != null) {
                    string whiteMatPath = AssetDatabase.GetAssetPath(_whiteMat);
                    EditorPrefs.SetString(SettingKeyWhiteMaterial, whiteMatPath);
                }
                
                Debug.Log("Settings Saved!");
            }

            if (GUILayout.Button("Take Snapshots")) 
                takeSnapshots();

            if (isStarted)
                if (GUILayout.Button("Terminate"))
                    stopFlag = true;
        }

        private async void takeSnapshots() {
            isStarted = true;

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
            else Debug.LogError("Failed to get or create SceneView.");

            await Task.Delay(1000);

            // 遍历选中的 FBX 资源
            foreach (var guid in selectedGuids) {
                if (stopFlag) {
                    isStarted = false;
                    stopFlag = false;
                    return;
                }

                string assetPath = AssetDatabase.GUIDToAssetPath(guid);
                // 只处理 FBX 文件
                if (Path.GetExtension(assetPath).ToLower() == ".fbx" ||
                    Path.GetExtension(assetPath).ToLower() == ".prefab") {
                    // 加载模型
                    GameObject modelPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(assetPath);
                    if (modelPrefab == null) {
                        Debug.LogWarning($"Asset at {assetPath} could not be loaded");
                        continue;
                    }

                    GameObject prefab = PrefabUtility.InstantiatePrefab(modelPrefab) as GameObject;
                    if (prefab == null) {
                        Debug.LogWarning($"Asset at {assetPath} could not be instantiated");
                        continue;
                    }

                    GameObject instance = new GameObject("Empty");
                    prefab.transform.SetParent(instance.transform);

                    # region Check Materials
                    async Task checkMaterials(float maxSize) {
                        Dictionary<string, List<(MeshRenderer meshRenderer, int index)>> matDict = new();
                        MeshRenderer[] renderers = instance.GetComponentsInChildren<MeshRenderer>();
                        foreach (MeshRenderer mr in renderers) {
                            Material[] mats = mr.sharedMaterials;
                            for (int i = 0; i < mats.Length; i++) {
                                if (mats[i] == null)
                                    continue;

                                string matName = mats[i].name;
                                if (!matDict.ContainsKey(matName)) {
                                    matDict[matName] = new List<(MeshRenderer, int)>();
                                }

                                matDict[matName].Add((mr, i));
                            }
                        }

                        if (matDict.Count == 0) {
                            Debug.LogWarning("Could not find any material on this object");
                            return;
                        }

                        // 遍历所有的材质组，每次以当前材质组作为主材质组（PrimaryMatName）
                        foreach (string primaryMatName in matDict.Keys) {
                            // 用于缓存所有需要还原的材质，结构为：MeshRenderer -> (材质索引 -> 原始材质)
                            Dictionary<MeshRenderer, Dictionary<int, Material>> backupMaterials =
                                new Dictionary<MeshRenderer, Dictionary<int, Material>>();

                            // 遍历所有分组
                            foreach (var kvp in matDict) {
                                string currentMatName = kvp.Key;
                                List<(MeshRenderer meshRenderer, int index)> list = kvp.Value;

                                foreach (var (mr, index) in list) {
                                    // 备份当前材质
                                    if (!backupMaterials.ContainsKey(mr))
                                        backupMaterials[mr] = new Dictionary<int, Material>();

                                    // 这里使用 mr.materials（而非 sharedMaterials）来操作实例材质
                                    Material[] currentMats = mr.sharedMaterials;
                                    backupMaterials[mr][index] = currentMats[index];

                                    // 如果当前材质组为主材质组，则替换为黄色基础材质；否则替换为白色基础材质
                                    if (currentMatName == primaryMatName)
                                        currentMats[index] = _yellowMat; // YellowMat 为预先定义好的黄色基础材质
                                    else currentMats[index] = _whiteMat; // WhiteMat 为预先定义好的白色基础材质

                                    mr.sharedMaterials = currentMats;
                                }
                            }

                            // 调用并 await 拍照方法，传入当前主材质组的名称
                            await takePictures(maxSize, primaryMatName);

                            // 拍照完成后，还原所有材质
                            foreach (var kvp in backupMaterials) {
                                MeshRenderer mr = kvp.Key;
                                Material[] currentMats = mr.sharedMaterials;
                                foreach (var indexAndMat in kvp.Value) {
                                    currentMats[indexAndMat.Key] = indexAndMat.Value;
                                }

                                mr.sharedMaterials = currentMats;
                            }
                        }

                    }
                    # endregion
                    
                    void screenshotAndSave(string direction, string materialId = "") {
                        string iconPath = _fbxIconPath;
                        Texture2D screenshot = CaptureSceneView(sceneView, _resolution, _resolution);
                        if (screenshot != null) {
                            // 如果设置的目录不存在，则创建
                            if (!Directory.Exists(iconPath))
                                Directory.CreateDirectory(iconPath);
                            // 保存PNG
                            string pngFullPath = Path.Combine(iconPath,
                                Path.GetFileNameWithoutExtension(assetPath) + $"=-=-={direction}");
                            if (materialId != "")
                                pngFullPath += $"=-=-={materialId}";
                            pngFullPath += ".png";
                            File.WriteAllBytes(pngFullPath, screenshot.EncodeToPNG());
                            AssetDatabase.Refresh();

                            // 将生成的PNG设置为FBX icon
                            Texture2D loadedIcon = AssetDatabase.LoadAssetAtPath<Texture2D>(pngFullPath);
                        }
                    }
                    
                    async Task takePictures(float maxSize, string matId = "") {
                        SceneRotationUtil.setSceneViewStandardView(sceneView, StandardView.Front);
                        for (int i = 0; i < 5; i++) {
                            sceneView.FrameSelected();
                            sceneView.Repaint();
                            await Task.Delay(700);
                            if (Mathf.Abs(maxSize - sceneView.size) <= 0.2f) break;
                        }
                        screenshotAndSave("f", matId);

                        SceneRotationUtil.setSceneViewStandardView(sceneView, StandardView.Back);
                        for (int i = 0; i < 5; i++) {
                            sceneView.FrameSelected();
                            sceneView.Repaint();
                            await Task.Delay(700);
                            if (Mathf.Abs(maxSize - sceneView.size) <= 0.2f) break;
                        }
                        screenshotAndSave("b", matId);

                        SceneRotationUtil.setSceneViewStandardView(sceneView, StandardView.Top);
                        for (int i = 0; i < 5; i++) {
                            sceneView.FrameSelected();
                            sceneView.Repaint();
                            await Task.Delay(700);
                            if (Mathf.Abs(maxSize - sceneView.size) <= 0.2f) break;
                        }
                        screenshotAndSave("t", matId);

                        SceneRotationUtil.setSceneViewStandardView(sceneView, StandardView.Bottom);
                        for (int i = 0; i < 5; i++) {
                            sceneView.FrameSelected();
                            sceneView.Repaint();
                            await Task.Delay(700);
                            if (Mathf.Abs(maxSize - sceneView.size) <= 0.2f) break;
                        }
                        screenshotAndSave("o", matId);

                        SceneRotationUtil.setSceneViewStandardView(sceneView, StandardView.Left);
                        for (int i = 0; i < 5; i++) {
                            sceneView.FrameSelected();
                            sceneView.Repaint();
                            await Task.Delay(700);
                            if (Mathf.Abs(maxSize - sceneView.size) <= 0.2f) break;
                        }
                        screenshotAndSave("l", matId);

                        SceneRotationUtil.setSceneViewStandardView(sceneView, StandardView.Right);
                        for (int i = 0; i < 5; i++) {
                            sceneView.FrameSelected();
                            sceneView.Repaint();
                            await Task.Delay(700);
                            if (Mathf.Abs(maxSize - sceneView.size) <= 0.2f) break;
                        }
                        screenshotAndSave("r", matId);
                    }

                    // 选中它并聚焦
                    Selection.activeGameObject = instance;
                    sceneView.FrameSelected();
                    sceneView.Repaint();
                    await Task.Delay(700);
                    float frameSize1 = sceneView.size;
                    sceneView.FrameSelected();
                    sceneView.Repaint();
                    await Task.Delay(700);
                    float frameSize2 = sceneView.size;
                    float maxSize = Mathf.Max(frameSize1, frameSize2);
                    
                    if (sceneView != null) {
                        if (!_inspectMaterial)
                            await takePictures(maxSize);
                        else {
                            // 黄色的基础材质为YellowMat，白色的基础材质为WhiteMat
                            // 遍历 instance (GameObject) 以及它的所有children，如果发现了 MeshRenderer
                            // 则遍历 MeshRenderer 中的所有材质，并将材质添加到 MatDictionary 字典中（材质名是 Key，(MeshRenderer, index) 是 value）
                            // 遍历 MatDictionary，对于每一个Item(1)，都缓存一下当前在里面的材质，并且吧黄色基础材质赋予给这个index
                            // 随后，遍历 MapDictionary 中的所有其他材质(Item(2))，缓存一下当前在里面的材质，并且吧白色基础材质赋予给这个index
                            // 调用并await takePictures("{Item1的材质名}");
                            // 随后，遍历 MapDictionary，并将刚才赋予的临时材质用缓存的原来的材质替换掉（也就是还原回去）
                            await checkMaterials(maxSize);
                        }
                    }
                    else {
                        Debug.LogError("Failed to get or create SceneView.");
                        return;
                    }

                    // 销毁临时实例
                    DestroyImmediate(instance);
                }
            }

            // 恢复当前场景
            EditorSceneManager.OpenScene(currentScenePath);
            // 删除新场景
            if (_scenePath == "")
                AssetDatabase.DeleteAsset(newScenePath);
            // 刷新
            AssetDatabase.Refresh();

            isStarted = false;
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
                DestroyImmediate(tempRT);
            }
        }
    }

}

# endif