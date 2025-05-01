using System.IO;
using System.Reflection;

namespace Fries.Inspector.SceneBehaviours {
    # if UNITY_EDITOR
    using UnityEditor;
    using UnityEngine;
    using System;
    using System.Linq;
    using System.Collections.Generic;

    [CustomEditor(typeof(SceneSelectionProxy))]
    public class SceneProxyEditor : Editor {
        private GUIContent _sceneHeaderContent;
        private GUIStyle _sceneHeaderStyle;
        private Texture2D _headerBgTex;
        private void CacheSceneHeader() {
            var proxy = (SceneSelectionProxy)target;
            string sceneName = proxy.sceneName.Split("\u00a6")[^1].Split(".unity")[0];
            Texture sceneIcon = EditorGUIUtility.IconContent("SceneAsset Icon").image;
            _sceneHeaderContent = new GUIContent(sceneName, sceneIcon);
            
            var skin = EditorGUIUtility.GetBuiltinSkin(EditorSkin.Inspector);
            _sceneHeaderStyle = new GUIStyle {
                // fontStyle         = FontStyle.Bold,              // 加粗
                normal            = { textColor = Color.white }, // 白色文字
                alignment         = TextAnchor.UpperLeft,        // 左上对齐
                imagePosition     = ImagePosition.ImageLeft,     // 图标在文字左侧
            };
        }

        protected override void OnHeaderGUI() {
            // 每次 Inspector 刷新都走这里，确保缓存没过期
            if (_sceneHeaderContent == null)
                CacheSceneHeader();

            // 申请一个 60 高的区域
            Rect headerRect = GUILayoutUtility.GetRect(
                GUIContent.none,
                _sceneHeaderStyle,
                GUILayout.Height(60),
                GUILayout.ExpandWidth(true)
            );

            // 先画背景色（在所有事件里都生效）
            EditorGUI.DrawRect(headerRect, new Color(60f/255f, 60f/255f, 60f/255f));

            Rect lineRect = new Rect(
                headerRect.x,
                headerRect.yMax - 2f,
                headerRect.width,
                2f
            );
            EditorGUI.DrawRect(lineRect, new Color(26f/255f, 26f/255f, 26f/255f));
            
            // 只在 Repaint 事件时，由 GUIStyle 真正绘制图标+文字，避免“Style.Draw may not be called”报错
            if (Event.current.type == EventType.Repaint) {
                float iconW = 30f;
                float iconH = 30f;
                // 图标绘制区域：左上角，y 直接使用 headerRect.y
                Rect iconRect = new Rect(
                    headerRect.x + 10,
                    headerRect.y + 10, 
                    iconW,
                    iconH
                );
                GUI.DrawTexture(iconRect, (Texture2D)_sceneHeaderContent.image, ScaleMode.ScaleToFit);

                // 文字绘制区域：紧跟图标右侧 + 少许间隔
                float textX = iconRect.x + iconRect.width + 4; 
                Rect textRect = new Rect(
                    textX,
                    headerRect.y + 10,
                    headerRect.width - (textX - headerRect.x),
                    headerRect.height
                );
                // 用 GUIStyle 只绘制文字
                _sceneHeaderStyle.Draw(
                    textRect,
                    new GUIContent(_sceneHeaderContent.text),
                    false, false, false, false
                );
            }
        }
        
        
        private readonly Dictionary<SceneBehaviour, Editor> behaviourEditors = new();

        // 缓存所有 SceneBehaviour 的子类
        private List<Type> behaviourTypes;

        private static GUIStyle _inspectorTitlebarStyle;
        private void OnEnable() {
            behaviourTypes = TypeCache.GetTypesDerivedFrom<SceneBehaviour>().ToList();
        }

        public override void OnInspectorGUI() {
            serializedObject.Update();
            GUILayout.Space(4);

            // 尝试取内置的 “Add Component” 按钮样式，如果没找到就退回到 miniButtonMid
            GUIStyle addButtonStyle = GUI.skin.FindStyle("AC Button");
            float btnHeight = addButtonStyle.fixedHeight > 0 ? addButtonStyle.fixedHeight : 18;

            var proxy = (SceneSelectionProxy)target;
            var instances = proxy.getSceneBehaviours();
            foreach (var beh in instances) {
                if (beh == null) continue;

                // Get or create a cached Editor for this behaviour instance
                if (!behaviourEditors.TryGetValue(beh, out var behEditor) || behEditor == null) {
                    CreateCachedEditor(beh, null, ref behEditor);
                    behaviourEditors[beh] = behEditor;
                }

                drawDivider(proxy, beh);

                if (!_foldouts[beh]) continue;
                // Draw its inspector UI
                EditorGUILayout.BeginVertical(GUI.skin.box);
                behEditor.OnInspectorGUI();
                EditorGUILayout.EndVertical();
                GUILayout.Space(4);
            }

            // 把按钮包在一个 Horizontal 里，两侧用 FlexibleSpace
            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("Add Component", addButtonStyle, GUILayout.Height(btnHeight))) {
                // 点击后拿到刚刚布局出的按钮位置
                Rect rect = GUILayoutUtility.GetLastRect();
                PopupWindow.Show(rect, new SceneBehaviourPopup((SceneSelectionProxy)target, behaviourTypes));
            }

            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();

            serializedObject.ApplyModifiedProperties();
        }

        private Dictionary<SceneBehaviour, bool> _foldouts = new();
        private void drawDivider(SceneSelectionProxy proxy, SceneBehaviour beh) {
            if (!_foldouts.TryGetValue(beh, out var isOpen)) {
                isOpen = true;
                _foldouts[beh] = true;
            }

            // 先拿一行的 Rect
            Rect headerRect = GUILayoutUtility.GetRect(0, EditorGUIUtility.singleLineHeight, GUILayout.ExpandWidth(true));
            // 用 inspectorTitlebar 样式画背景
            GUI.Box(headerRect, GUIContent.none);

            // 折叠箭头 + 名字
            Rect foldoutRect = new Rect(headerRect.x + 16, headerRect.y, headerRect.width - 24, headerRect.height);
            isOpen = EditorGUI.Foldout(foldoutRect, isOpen,
                ObjectNames.NicifyVariableName(beh.GetType().Name), true);
            _foldouts[beh] = isOpen;
            
            // 右侧小点点菜单按钮
            Rect menuRect = new Rect(headerRect.xMax - 18, headerRect.y, 18, headerRect.height);
            if (GUI.Button(menuRect, EditorGUIUtility.IconContent("Icon Dropdown"), EditorStyles.iconButton)) {
                var menu = new GenericMenu();
                menu.AddItem(new GUIContent("Remove Behaviour"), false, () => {
                    // 调用你自己的移除方法
                    proxy.removeBehaviour(beh);
                });
                menu.DropDown(menuRect);
            }
        }

        private GUIStyle getTitleBar() {
            var editorStylesType = typeof(EditorStyles);
            var prop = editorStylesType.GetProperty("inspectorTitlebar",
                BindingFlags.NonPublic | BindingFlags.Static);
            if (prop != null)
                return prop.GetValue(null, null) as GUIStyle;
            var field = editorStylesType.GetField("inspectorTitlebar",
                BindingFlags.NonPublic | BindingFlags.Static);
            return field?.GetValue(null) as GUIStyle;
        }

        // 内部类：负责绘制弹窗内容
        private class SceneBehaviourPopup : PopupWindowContent {
            private SceneSelectionProxy proxy;
            private List<Type> types;
            private string search = "";
            private Vector2 scrollPos;

            public SceneBehaviourPopup(SceneSelectionProxy proxy, List<Type> types) {
                this.proxy = proxy;
                this.types = types;
            }

            public override Vector2 GetWindowSize() {
                return new Vector2(300, 400);
            }

            public override void OnGUI(Rect rect) {
                // 顶部搜索框
                EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
                search = EditorGUILayout.TextField(search, EditorStyles.toolbarSearchField);
                if (GUILayout.Button("×", EditorStyles.toolbarButton, GUILayout.Width(20))) {
                    search = "";
                    GUI.FocusControl(null);
                }

                EditorGUILayout.EndHorizontal();

                // 列表
                scrollPos = EditorGUILayout.BeginScrollView(scrollPos);
                foreach (var t in types) {
                    if (string.IsNullOrEmpty(search) ||
                        t.Name.IndexOf(search, StringComparison.OrdinalIgnoreCase) >= 0) {
                        // 用 miniButton 保证与系统风格相近
                        if (GUILayout.Button(t.Name, EditorStyles.miniButton, GUILayout.ExpandWidth(true))) {
                            proxy.addBehaviour(t);
                            editorWindow.Close();
                        }
                    }
                }

                EditorGUILayout.EndScrollView();
            }
        }
    }
# endif
}