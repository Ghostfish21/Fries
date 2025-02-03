# if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.IO;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Fries.FbxPreviewFixer {

    [InitializeOnLoad]
    public static class ProjectWindowIconDrawer {
        // 缓存你想要显示的自定义图标
        private static Dictionary<string, Texture2D> customIcons = new Dictionary<string, Texture2D>();

        public static void setup() {
            EditorApplication.projectWindowItemOnGUI -= OnProjectWindowItemGUI;
            EditorApplication.projectWindowItemOnGUI += OnProjectWindowItemGUI;
            // 预先加载你的图标
            string iconPath = EditorPrefs.GetString("Fbx_Icon_Fixer.Fbx_Icon_Path", "Assets/Editor/Fbx Icons");
            Directory.CreateDirectory(iconPath);
            // 加载所有图标
            string[] iconPaths = Directory.GetFiles(iconPath, "*.png");
            foreach (string singleIconPath in iconPaths) {
                string fileName = Path.GetFileNameWithoutExtension(singleIconPath);
                Texture2D icon = AssetDatabase.LoadAssetAtPath<Texture2D>(singleIconPath);
                customIcons[fileName] = icon;
            }
        }


        private static void OnProjectWindowItemGUI(string guid, Rect selectionRect) {
            bool _isEnabled = EditorPrefs.GetBool("Fbx_Icon_Fixer.Is_Enabled", true);
            if (!_isEnabled) return;

            if (customIcons.ContainsKey(guid)) {
                Texture2D icon = customIcons[guid];
                if (icon != null) {
                    bool isLargeIconMode = Mathf.Abs(selectionRect.width - selectionRect.height) < 20f;
                    Rect iconRect;
                    if (isLargeIconMode) iconRect = new Rect(selectionRect.x, selectionRect.y, selectionRect.height/5*4, selectionRect.height/5*4);
                    else iconRect = new Rect(selectionRect.x, selectionRect.y, selectionRect.height, selectionRect.height);
                    GUI.DrawTexture(iconRect, icon, ScaleMode.ScaleToFit);
                }
            }
            
        }
    }
}

# endif