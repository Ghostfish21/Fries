using System;
using System.Collections.Generic;
using System.Reflection;
using Fries.Pool;
using UnityEditor;

namespace Fries.Inspector.UndoRedoEvent {
# if UNITY_EDITOR
    [InitializeOnLoad]
# endif
    public class UndoRedoEvent {
        private static UndoRedoEvent ure;
        public static UndoRedoEvent inst => ure;

        static UndoRedoEvent() {
            ure = new UndoRedoEvent();
        }

        private Dictionary<string, DictList<MethodInfo>> data = new();

        public UndoPropertyModification[] tryTriggerEvent(UndoPropertyModification[] modifications) {
            // string fieldName = 
            // if (!data.ContainsKey(fieldName)) return null;
            // var methods = data[fieldName];
            // methods.Nullable().ForEach(method => {
            //     if (method.Name.Contains(type)) 
            // });
            return null;
        }
        
        public UndoRedoEvent(string[] assemblies = null) {
            data.Clear();

            ReflectionUtils.loopAssemblies(assembly => {
                Type[] types = assembly.GetTypes();
                types.ForEach(type => {
                    FieldInfo[] fields = type.GetFields();
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
                            data[f.Name] = methods;
                        }
                    });
                });
            });
        }
    }
}