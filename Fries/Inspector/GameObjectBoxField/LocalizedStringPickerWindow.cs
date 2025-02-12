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
                // 获取 LocalizedString 类型（注意命名空间及程序集名称需与实际一致）
                var localizedStringType = Type.GetType("UnityEngine.Localization.LocalizedString, Unity.Localization");
                if (localizedStringType == null)
                    throw new Exception("无法获取 UnityEngine.Localization.LocalizedString 类型，请确认 Localization 包已安装");

                // 创建 LocalizedString 实例
                var localizedStringInstance = Activator.CreateInstance(localizedStringType);
                if (localizedStringInstance == null)
                    throw new Exception("无法创建 LocalizedString 实例");

                // 获取 TableReference 类型
                var tableReferenceType =
                    Type.GetType("UnityEngine.Localization.Tables.TableReference, Unity.Localization");
                if (tableReferenceType == null)
                    throw new Exception("无法获取 TableReference 类型");

                // 通过隐式转换方法将 tableId (string) 转换为 TableReference 实例
                var opImplicitTableRef = tableReferenceType.GetMethod("op_Implicit", new Type[] { typeof(string) });
                if (opImplicitTableRef == null)
                    throw new Exception("TableReference 隐式转换方法 op_Implicit(string) 未找到");
                object tableReferenceObj = opImplicitTableRef.Invoke(null, new object[] { tableId });
                if (tableReferenceObj == null)
                    throw new Exception("无法创建 TableReference 实例");

                // 获取 TableEntryReference 类型
                var tableEntryReferenceType =
                    Type.GetType("UnityEngine.Localization.Tables.TableEntryReference, Unity.Localization");
                if (tableEntryReferenceType == null)
                    throw new Exception("无法获取 TableEntryReference 类型");

                // 通过隐式转换方法将 stringId (string) 转换为 TableEntryReference 实例
                var opImplicitTableEntryRef =
                    tableEntryReferenceType.GetMethod("op_Implicit", new Type[] { typeof(string) });
                if (opImplicitTableEntryRef == null)
                    throw new Exception("TableEntryReference 隐式转换方法 op_Implicit(string) 未找到");
                object tableEntryReferenceObj = opImplicitTableEntryRef.Invoke(null, new object[] { key });
                if (tableEntryReferenceObj == null)
                    throw new Exception("无法创建 TableEntryReference 实例");

                // 设置 LocalizedString 实例的 TableReference 属性
                var tableReferenceProp =
                    localizedStringType.GetProperty("TableReference", BindingFlags.Public | BindingFlags.Instance);
                if (tableReferenceProp == null)
                    throw new Exception("属性 TableReference 未找到");
                tableReferenceProp.SetValue(localizedStringInstance, tableReferenceObj);

                // 设置 LocalizedString 实例的 TableEntryReference 属性
                var tableEntryReferenceProp = localizedStringType.GetProperty("TableEntryReference",
                    BindingFlags.Public | BindingFlags.Instance);
                if (tableEntryReferenceProp == null)
                    throw new Exception("属性 TableEntryReference 未找到");
                tableEntryReferenceProp.SetValue(localizedStringInstance, tableEntryReferenceObj);

                // 获取 LocalizedString 实例的 GetLocalizedString 方法
                var getLocalizedStringMethod = localizedStringType.GetMethod("GetLocalizedString",
                    BindingFlags.Public | BindingFlags.Instance);
                if (getLocalizedStringMethod == null)
                    throw new Exception("方法 GetLocalizedString 未找到");

                // 调用 GetLocalizedString 方法（无参数调用）
                object localizedText = getLocalizedStringMethod.Invoke(localizedStringInstance, null);

                // 如果 T 为 string，则直接返回结果；否则根据需要进行转换
                return (T)localizedText;
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