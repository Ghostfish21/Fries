using System;
using System.Collections.Generic;
using System.Reflection;
using Fries.Pool;
# if UNITY_EDITOR
using UnityEditor;
# endif

namespace Fries.Inspector.UndoRedoEvent {
# if UNITY_EDITOR
    [InitializeOnLoad]
# endif
    public class UndoRedoEvent {
        private static UndoRedoEvent ure;
        public static UndoRedoEvent inst => ure;

        private static bool isRedo;
        static UndoRedoEvent() {
            ure = new UndoRedoEvent();
            Undo.undoRedoEvent += onUndoRedoEvent;
            Undo.postprocessModifications += ure.tryTriggerEvent;
        }

        // 这个回调在每次 Undo 或 Redo 完成后执行
        static void onUndoRedoEvent(in UndoRedoInfo info) {
            isRedo = info.isRedo;
        }

        private Dictionary<string, DictList<MethodInfo>> data = new();
        
# if UNITY_EDITOR
        public UndoPropertyModification[] tryTriggerEvent(UndoPropertyModification[] modifications) {
            if (!Undo.isProcessing)
                return modifications;
            
            foreach (var mod in modifications) {
                var targetObj = mod.currentValue.target;
                var type     = targetObj.GetType();
                var assembly = type.Assembly;
        
                string propertyPath = mod.previousValue.propertyPath;
                string fieldName    = propertyPath.Contains(".")
                    ? propertyPath.Substring(0, propertyPath.IndexOf('.'))
                    : propertyPath;
        
                string fullFieldName = $"{assembly.FullName}::{type.Name}::{fieldName}";
                
                if (!data.ContainsKey(fullFieldName)) continue;
                var methods = data[fieldName];
                string opType = isRedo ? "Redo" : "Undo";
                methods.Nullable().ForEach(method => {
                    if (opType == "Undo") {
                        if (method.Name.Contains("Undo"))
                            modifications =
                                method.Invoke(null, new Object[] { modifications }) as UndoPropertyModification[];
                    }
                    else if (opType == "Redo") {
                        if (method.Name.Contains("Redo"))
                            modifications =
                                method.Invoke(null, new Object[] { modifications }) as UndoPropertyModification[];
                    }
                });
            }
            return modifications;
        }
# endif

        public UndoRedoEvent(string[] assemblies = null) {
            data.Clear();
# if UNITY_EDITOR
            ReflectionUtils.loopAssemblies(assembly => {
                Type[] types = assembly.GetTypes();
                types.ForEach(type => {
                    FieldInfo[] fields = type.GetFields(BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
                    fields.ForEach(f => {
                        var attributes = f.GetCustomAttributes(typeof(OnUndoRedoAttribute), false);
                        if (attributes.Nullable().Length != 0) {
                            string fieldName = f.Name;
                            DictList<string> methodsToFind = new();
                            string undoName = fieldName + "Undo";
                            string redoName = fieldName + "Redo";
                            string bothName = fieldName + "UndoRedo";
                            methodsToFind.Add(undoName);
                            methodsToFind.Add(redoName);
                            methodsToFind.Add(bothName);
                            attributes.ForEach(attr => {
                                OnUndoRedoAttribute attrU = (OnUndoRedoAttribute)attr;
                                if (attrU.groupId == null) return;
                                string groupUndo = attrU.groupId + "Undo";
                                string groupRedo = attrU.groupId + "Redo";
                                string groupBoth = attrU.groupId + "UndoRedo";
                                methodsToFind.Add(groupUndo);
                                methodsToFind.Add(groupRedo);
                                methodsToFind.Add(groupBoth);
                            });

                            DictList<MethodInfo> methods = new DictList<MethodInfo>();
                            methodsToFind.ForEach(str => {
                                MethodInfo methodInfo = type.GetMethod(str);
                                if (methodInfo == null) return;
                                if (methodInfo.checkSignature(typeof(UndoPropertyModification[]), typeof(UndoPropertyModification[])))
                                    methods.Add(methodInfo);
                            });
                            string fullFieldName = assembly.FullName + "::" + type.Name + "::" + f.Name;
                            data[fullFieldName] = methods;
                        }
                    });
                });
            });
        }
# endif
    }
}