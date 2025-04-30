using System;
using System.Collections;
using System.Reflection;
using Fries.Data;
using Fries.Pool;

namespace Fries.Inspector.GameObjectBoxField {
# if UNITY_EDITOR
    using System.Collections.Generic;
    using System.Linq;
    using UnityEditor;
    using UnityEngine;

    public class LocalizedStringPickerWindow : EditorWindow {
        private int controlID;
        private SerializedProperty property;
        private PickerData data;

        // 搜索关键词和滚动位置
        private string searchString = "";
        private Vector2 scrollPos;

        // 从 Localization 包中加载的真实本地化字符串列表
        private DictList<LocalizedStringKey> localizedKeys = new();
        
        private List<LocalizedStringKey> filteredKeys;

        /// <summary>
        /// 初始化选择器数据，并加载真实的数据源
        /// </summary>
        /// <param name="controlID">用于标识本次选择器的 ID</param>
        /// <param name="property">目标 SerializedProperty（类型为 string）</param>
        /// <param name="data">相关 PickerData 数据</param>
        public void Init(int controlID, SerializedProperty property, PickerData data) {
            this.controlID = controlID;
            this.property = property;
            this.data = data;
            LoadLocalizedKeys();
            UpdateFilteredList();
        }

        /// <summary>
        /// 从所有 StringTableCollection 中加载所有的 key
        /// </summary>
        private void LoadLocalizedKeys() {
            localizedKeys.Clear();
            
            // 获取 Unity.Localization.Editor 程序集
            Assembly localizationAssembly = AppDomain.CurrentDomain.GetAssemblies()
                .FirstOrDefault(a => a.GetName().Name == "Unity.Localization.Editor");
            if (localizationAssembly == null) return;
            
            // 获取 LocalizationEditorSettings 类型
            Type localizationEditorSettingsType =
                localizationAssembly.GetType("UnityEditor.Localization.LocalizationEditorSettings");
            if (localizationEditorSettingsType == null) return;
            
            // 获取静态方法 GetStringTableCollections
            MethodInfo getCollectionsMethod = localizationEditorSettingsType.GetMethod(
                "GetStringTableCollections",
                BindingFlags.Public | BindingFlags.Static
            );
            if (getCollectionsMethod == null) return;
            
            // 调用方法获得集合，返回值类型未知，通常为 IEnumerable
            object result = getCollectionsMethod.Invoke(null, null);
            if (result is not IEnumerable collections) return;
            
            foreach (object collection in collections) {
                if (collection == null)
                    continue;

                // 通过反射获取 collection 的 SharedData 属性
                PropertyInfo sharedDataProperty = collection.GetType()
                    .GetProperty("SharedData", BindingFlags.Public | BindingFlags.Instance);
                if (sharedDataProperty == null)
                    continue;

                object sharedData = sharedDataProperty.GetValue(collection, null);
                if (sharedData == null)
                    continue;
                
                // 尝试从 sharedData 获取 TableId 属性
                string tableId = null;
                PropertyInfo tableIdProperty = sharedData.GetType()
                    .GetProperty("TableCollectionName", BindingFlags.Public | BindingFlags.Instance);
                if (tableIdProperty != null) {
                    object tableIdObj = tableIdProperty.GetValue(sharedData, null);
                    if (tableIdObj is string s) {
                        tableId = s;
                    }
                }

                // 获取 SharedData 中的 Entries 属性，Entries 应该是个 IEnumerable
                PropertyInfo entriesProperty = sharedData.GetType()
                    .GetProperty("Entries", BindingFlags.Public | BindingFlags.Instance);
                if (entriesProperty == null)
                    continue;

                object entriesObj = entriesProperty.GetValue(sharedData, null);
                if (entriesObj is not IEnumerable entries) continue;
                foreach (object entry in entries) {
                    if (entry == null)
                        continue;

                    // 获取 entry 的 Key 属性
                    PropertyInfo keyProperty = entry.GetType()
                        .GetProperty("Key", BindingFlags.Public | BindingFlags.Instance);
                    if (keyProperty == null)
                        continue;

                    object keyValue = keyProperty.GetValue(entry, null);
                    if (keyValue is string keyStr && !localizedKeys.Contains(new LocalizedStringKey(tableId, keyStr))) {
                        localizedKeys.Add(new LocalizedStringKey(tableId, keyStr));
                    }
                }
            }

        }

        private void OnGUI() {
            GUILayout.Label("Pick Localized String", EditorStyles.boldLabel);

            // 搜索输入框
            EditorGUI.BeginChangeCheck();
            searchString = EditorGUILayout.TextField("Search", searchString);
            if (EditorGUI.EndChangeCheck()) {
                UpdateFilteredList();
            }

            // 显示过滤后的字符串列表
            scrollPos = EditorGUILayout.BeginScrollView(scrollPos);
            if (filteredKeys == null) filteredKeys = new List<LocalizedStringKey> { null };
            foreach (LocalizedStringKey key in filteredKeys) {
                if (key == null) {
                    if (GUILayout.Button("Null", EditorStyles.miniButton))
                        OnSelect(null);
                }
                else {
                    if (GUILayout.Button($"{key.tableId}: {key.key}", EditorStyles.miniButton)) 
                        OnSelect(key);
                }
            }

            EditorGUILayout.EndScrollView();
        }

        /// <summary>
        /// 根据搜索关键词更新过滤后的列表
        /// </summary>
        private void UpdateFilteredList() {
            if (string.IsNullOrEmpty(searchString)) {
                filteredKeys = new List<LocalizedStringKey> { null };
                foreach (LocalizedStringKey localizedStringKey in localizedKeys) 
                    filteredKeys.Add(localizedStringKey);
            }
            
            else {
                filteredKeys = localizedKeys
                    .Where(x => x == null ||
                                x.tableId.ToLower().Contains(searchString.ToLower()) || x.key.ToLower().Contains(searchString.ToLower()))
                    .ToList();
            }
        }

        /// <summary>
        /// 当选中某个字符串时，将值写入 SerializedProperty 并关闭窗口
        /// </summary>
        private void OnSelect(LocalizedStringKey key) {
            property.managedReferenceValue = key;
            property.serializedObject.ApplyModifiedProperties();
            Close();
        }
    }
# endif
}