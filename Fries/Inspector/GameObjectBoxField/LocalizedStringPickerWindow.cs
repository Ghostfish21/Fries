using System;
using System.Collections;
using System.Reflection;
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

        private class LocalizedStringKey : SerializableSysObject {
            public string tableId;
            public string key;

            public LocalizedStringKey(string tableId, string key) {
                this.tableId = tableId;
                this.key = key;
            }
            
            public override bool Equals(object obj) {
                if (obj is not LocalizedStringKey other)
                    return false;
                return tableId == other.tableId && key == other.key;
            }

            public override int GetHashCode() {
                int hash = 17;
                hash = hash * 23 + (tableId != null ? tableId.GetHashCode() : 0);
                hash = hash * 23 + (key != null ? key.GetHashCode() : 0);
                return hash;
            }

            public static bool operator ==(LocalizedStringKey a, LocalizedStringKey b) {
                if (ReferenceEquals(a, b))
                    return true;
                if (a is null || b is null)
                    return false;
                return a.Equals(b);
            }

            public static bool operator !=(LocalizedStringKey a, LocalizedStringKey b) {
                return !(a == b);
            }

            public override string ToString() {
                return $"{tableId}: {key}";
            }

            public override T get<T>() {
                // 获取 LocalizationSettings 类型，注意命名空间及程序集名称需与实际一致
                var localizationSettingsType = Type.GetType("UnityEngine.Localization.Settings.LocalizationSettings, Unity.Localization");
                if (localizationSettingsType == null)
                    throw new Exception("Unable to access to UnityEngine.Localization.Settings.LocalizationSettings Type, please confirm Localization package is installed");

                // 获取静态属性 Instance
                var instanceProp = localizationSettingsType.GetProperty("Instance", BindingFlags.Public | BindingFlags.Static);
                if (instanceProp == null)
                    throw new Exception("Property LocalizationSettings.Instance is not found");
    
                var localizationSettingsInstance = instanceProp.GetValue(null);
                if (localizationSettingsInstance == null)
                    throw new Exception("LocalizationSettings.Instance is null");

                // 获取实例属性 StringDatabase
                var stringDatabaseProp = localizationSettingsType.GetProperty("StringDatabase", BindingFlags.Public | BindingFlags.Static);
                if (stringDatabaseProp == null)
                    throw new Exception("Property StringDatabase is not found");

                var stringDatabase = stringDatabaseProp.GetValue(localizationSettingsInstance);
                if (stringDatabase == null)
                    throw new Exception("StringDatabase is null。");

                // 获取 StringDatabase 类型的 GetLocalizedString(string, string) 方法
                var stringDatabaseType = stringDatabase.GetType();
                var methods = stringDatabaseType.GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                foreach (var methodInfo in methods) {
                    Debug.Log(methodInfo.ToString());
                }
                var getLocalizedStringMethod = stringDatabaseType.GetMethod("GetLocalizedString", new Type[] { typeof(string), typeof(string) });
                if (getLocalizedStringMethod == null)
                    throw new Exception("Method GetLocalizedString is not found");

                // 调用 GetLocalizedString 方法，传入 tableId 和 key
                object result = getLocalizedStringMethod.Invoke(stringDatabase, new object[] { tableId, key });
    
                // 如果 T 是 string，则直接返回结果，否则尝试转换（可能需要扩展其他类型支持）
                return (T) result;
            }
        }
        
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