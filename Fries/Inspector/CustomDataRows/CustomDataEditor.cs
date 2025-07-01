# if UNITY_EDITOR
using System;
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using Fries.Inspector;

namespace Fries.Inspector.CustomDataRows {

    [CustomEditor(typeof(CustomData))]
    public class CustomDataEditor : Editor {
        private CustomData _targetData;
        private string _newVariableName = "";
        // private CustomDataType _typeToAdd;

        // For storing the SerializedProperty of the dataStore for robust Undo/Redo and prefab modification support
        private SerializedProperty _dataStoreProperty;
        private SerializedProperty _runtimeDataStoreProperty;

        private void OnEnable() {
            _targetData = (CustomData)target;
            _dataStoreProperty = serializedObject.FindProperty("dataStore");
        }

        public override void OnInspectorGUI() {
            serializedObject.Update(); // Always start with this
            
            // "Add Row" button
            EditorGUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            bool showAddVariablePopup = false;
            if (GUILayout.Button("Add Data", GUI.skin.FindStyle("AC Button"))) {
                showAddVariablePopup = true;
                _newVariableName = ""; // Reset name field when opening popup
            }
            GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();

            if (_dataStoreProperty == null || !_dataStoreProperty.isArray) {
                EditorGUILayout.HelpBox("Data store not found or is not a list.", MessageType.Error);
                serializedObject.ApplyModifiedProperties();
                return;
            }

            if (_dataStoreProperty.arraySize == 0) {
                EditorGUILayout.LabelField("No persistent variables added yet.", EditorStyles.centeredGreyMiniLabel);
            }
            else {
                // Iterate through the SerializedProperty array elements
                for (int i = 0; i < _dataStoreProperty.arraySize; i++) {
                    SerializedProperty itemProperty = _dataStoreProperty.GetArrayElementAtIndex(i);
                    SerializedProperty nameProperty = itemProperty.FindPropertyRelative("name");
                    SerializedProperty typeProperty = itemProperty.FindPropertyRelative("type");
                    SerializedProperty valueProperty = itemProperty.FindPropertyRelative("value");
                    if (!CustomDataTypes.cachedTypes.ContainsKey(typeProperty.stringValue))
                        CustomDataTypes.init();
                    CustomDataType cdt = CustomDataTypes.cachedTypes[typeProperty.stringValue];

                    EditorGUILayout.BeginVertical(GUI.skin.box);
                    EditorGUILayout.BeginHorizontal();
                    // Draw the appropriate field based on type
                    float height = EditorGUI.GetPropertyHeight(valueProperty, true);
                    Rect rect = EditorGUILayout.GetControlRect(true, height);
                    rect.width -= 18;
                    string value = $"{nameProperty.stringValue} ({cdt.getDisplayName()})";
                    bool copyToRuntime = itemProperty.FindPropertyRelative("shouldCopyToRuntime").boolValue;
                    if (copyToRuntime) value = "    " + value;
                    EditorGUI.PropertyField(rect, valueProperty, new GUIContent(value), true);

                    if (copyToRuntime) {
                        var pkg = UnityEditor.PackageManager.PackageInfo.FindForAssembly(typeof(CustomDataEditor).Assembly);
                        Texture2D icon = AssetDatabase.LoadAssetAtPath<Texture2D>($"{pkg.assetPath}/Fries/Icons/Runtime.png");
                        var iconSize = EditorGUIUtility.singleLineHeight;
                        var iconRect = new Rect(rect.x, rect.y, iconSize, iconSize);
                        GUI.DrawTexture(iconRect, icon);
                    }

                    Rect menuRect = new Rect(rect.xMax, rect.y, 18, rect.height);
                    int prevDepth = GUI.depth;
                    GUI.depth = - 10;
                    if (GUI.Button(menuRect, EditorGUIUtility.IconContent("Icon Dropdown"), EditorStyles.iconButton)) {
                        GUI.depth = prevDepth;
                        var menu = new GenericMenu();
                        var i1 = i;
                        menu.AddItem(new GUIContent("Remove Data"), false, () => {
                            if (EditorUtility.DisplayDialog("Confirm Delete",
                                    $"Are you sure you want to delete variable '{nameProperty.stringValue}'", "Delete",
                                    "Cancel")) {
                                _dataStoreProperty.DeleteArrayElementAtIndex(i1);
                                serializedObject.ApplyModifiedProperties();
                            }
                        });
                        
                        bool b = _dataStoreProperty.GetArrayElementAtIndex(i1).FindPropertyRelative("shouldCopyToRuntime")
                            .boolValue;
                        if (!b) {
                            menu.AddItem(new GUIContent("Copy to Runtime"), false, () => {
                                _dataStoreProperty.GetArrayElementAtIndex(i1)
                                    .FindPropertyRelative("shouldCopyToRuntime")
                                    .boolValue = true;
                                serializedObject.ApplyModifiedProperties();
                            });
                        }
                        else {
                            menu.AddItem(new GUIContent("Don't Copy to Runtime"), false, () => {
                                _dataStoreProperty.GetArrayElementAtIndex(i1)
                                    .FindPropertyRelative("shouldCopyToRuntime")
                                    .boolValue = false;
                                serializedObject.ApplyModifiedProperties();
                            });
                        }

                        menu.DropDown(menuRect);
                    }
                    EditorGUILayout.EndHorizontal();
                    EditorGUILayout.EndVertical();
                }
            }

            _runtimeDataStoreProperty = serializedObject.FindProperty("runtimeDataStore");
            if (_runtimeDataStoreProperty.arraySize != 0) {
                for (int i = 0; i < _runtimeDataStoreProperty.arraySize; i++) {
                    SerializedProperty itemProperty = _runtimeDataStoreProperty.GetArrayElementAtIndex(i);
                    SerializedProperty nameProperty = itemProperty.FindPropertyRelative("name");
                    SerializedProperty typeProperty = itemProperty.FindPropertyRelative("type");
                    SerializedProperty valueProperty = itemProperty.FindPropertyRelative("valueStr");

                    EditorGUILayout.BeginVertical(GUI.skin.box);
                    EditorGUILayout.BeginHorizontal();
                    // Draw the appropriate field based on type
                    float height = 2*EditorGUIUtility.singleLineHeight;
                    Rect rect = EditorGUILayout.GetControlRect(true, height);
                    rect.width -= 18;
                    
                    Color bg = new Color(38/255f, 36/255f, 56/255f);
                    EditorGUI.DrawRect(rect, bg);
                    float lineH = EditorGUIUtility.singleLineHeight;
                    Rect line1 = new Rect(rect.x + 4, rect.y + 2, rect.width - 8, lineH);
                    EditorGUI.LabelField(line1, $"{nameProperty.stringValue} ({typeProperty.stringValue})", EditorStyles.boldLabel);
                    Rect line2 = new Rect(rect.x + 4, rect.y + 2 + lineH, rect.width - 8, lineH);
                    EditorGUI.LabelField(line2, valueProperty.stringValue);
                    
                    EditorGUILayout.EndHorizontal();
                    EditorGUILayout.EndVertical();
                }
            }

            serializedObject.ApplyModifiedProperties(); // Always end with this to save changes
            
            if (showAddVariablePopup) {
                Rect rect = EditorGUILayout.GetControlRect(false, 0);
                PopupWindow.Show(rect, new CustomDataPopup(promptForName));
            }
        }

        private void promptForName(string type) {
            // Use a modal window for name input for better UX
            VariableNameWindow.ShowWindow((newName) => {
                if (string.IsNullOrWhiteSpace(newName)) {
                    EditorUtility.DisplayDialog("Invalid Name", "Variable name cannot be empty or whitespace.", "OK");
                    return;
                }

                // Check for existing name using the SerializedProperty for consistency
                bool nameExists = false;
                for (int i = 0; i < _dataStoreProperty.arraySize; i++) {
                    if (_dataStoreProperty.GetArrayElementAtIndex(i).FindPropertyRelative("name").stringValue ==
                        newName) {
                        nameExists = true;
                        break;
                    }
                }

                if (nameExists) {
                    EditorUtility.DisplayDialog("Creation Failed", "This name is already in use.", "OK");
                }
                else {
                    // Add new item using SerializedProperty
                    int newIndex = _dataStoreProperty.arraySize;
                    _dataStoreProperty.InsertArrayElementAtIndex(newIndex);
                    SerializedProperty newItemProperty = _dataStoreProperty.GetArrayElementAtIndex(newIndex);
                    newItemProperty.managedReferenceValue = new CustomDataItem(newName, type);

                    serializedObject.ApplyModifiedProperties();
                    EditorUtility.SetDirty(_targetData); // Mark target as dirty to ensure changes are saved
                }
            });
        }
    }

// Helper window for entering the variable name
    public class VariableNameWindow : EditorWindow {
        private string _variableName = "";
        private System.Action<string> _onConfirm;
        private static GUIStyle buttonStyle;

        public static void ShowWindow(System.Action<string> onConfirm) {
            buttonStyle = new GUIStyle(GUI.skin.FindStyle("AC Button")) {
                fixedWidth = 0,     // 去掉任何固定宽度
                stretchWidth = true // 允许横向拉伸
            };
            
            VariableNameWindow window = GetWindow<VariableNameWindow>(true, "Enter Variable Name", true);
            window.minSize = new Vector2(300, 72);
            window.maxSize = new Vector2(300, 72);
            window._onConfirm = onConfirm;
            window.ShowModal();
        }

        private void OnGUI() {
            EditorGUILayout.LabelField("Please enter a name for the new variable:", EditorStyles.wordWrappedLabel);
            _variableName = EditorGUILayout.TextField("Variable Name:", _variableName);

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Confirm", buttonStyle, GUILayout.ExpandWidth(true))) {
                _onConfirm?.Invoke(_variableName);
                Close();
            }

            if (GUILayout.Button("Cancel", buttonStyle, GUILayout.ExpandWidth(true))) {
                Close();
            }
            EditorGUILayout.EndHorizontal();
        }
    }

    public class CustomDataPopup : PopupWindowContent {
        private string search = "";
        private Vector2 scrollPos;
        private readonly Action<string> promptForName;

        public CustomDataPopup(Action<string> promptForName) {
            this.promptForName = promptForName;
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
            foreach (var cdt in CustomDataTypes.cachedTypes.Values) {
                if (string.IsNullOrEmpty(search) ||
                    cdt.getDisplayName().IndexOf(search, StringComparison.OrdinalIgnoreCase) >= 0) {
                    // 用 miniButton 保证与系统风格相近
                    if (GUILayout.Button(cdt.getDisplayName(), EditorStyles.miniButton, GUILayout.ExpandWidth(true))) {
                        promptForName(cdt.getType().ToString());
                        editorWindow.Close();
                    }
                }
            }

            EditorGUILayout.EndScrollView();
        }
    }
    
}
# endif