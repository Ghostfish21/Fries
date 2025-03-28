using System;
using System.Reflection;

namespace Fries.Inspector.GameObjectBoxField {
    public class LocalizedStringKey : SerializableSysObject {
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

            // 如果 T 为 string，则直接返回结果；否则根据需要进行转换
            return (T)localizedStringInstance;
        }
    }
}