using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Fries.Data;
using Fries.Inspector;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Fries.EventFunctions {
    public class EventFunctionSystem : MonoBehaviour {
        private static EventFunctionSystem or;
        public static EventFunctionSystem inst => or;

        public string[] loadAssemblies;
        
        private void Awake() {
            if (or) {
                Destroy(gameObject);
                return;
            }

            or = this;
            
            ReflectionUtils.forStaticMethods((mi, de) => {
                string methodName = mi.Name;
                Type[] argsTypes = mi.GetParameters().Select(p => p.ParameterType).ToArray();
                eventFunctionsArgTypes[methodName] = argsTypes;
            }, typeof(EventFunctionDeclarer), BindingFlags.Public | BindingFlags.NonPublic, typeof(void), loadAssemblies);
        }

        private void Update() {
            trigger("update");
        }
        private void FixedUpdate() {
            trigger("fixedUpdate");
        }
        private void LateUpdate() {
            trigger("lateUpdate");
        }

        [EventFunctionDeclarer]
        public static void update() {}
        [EventFunctionDeclarer]
        public static void fixedUpdate() {}
        [EventFunctionDeclarer]
        public static void lateUpdate() {}

        private readonly Dictionary<string, Type[]> eventFunctionsArgTypes = new();
        private readonly Dictionary<string, Dictionary<object, Action<object[]>>> data = new();
        
        public void record(object obj) {
            MethodInfo[] allMethods = obj.GetType().GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            MethodInfo[] methodsWithEventFunctionAttr = Array.FindAll(allMethods, m => m.GetCustomAttribute<EventFunctionAttribute>() != null);
            foreach (MethodInfo method in methodsWithEventFunctionAttr) {
                string methodName = method.Name;
                Type[] argsTypes = method.GetParameters().Select(p => p.ParameterType).ToArray();
                if (!eventFunctionsArgTypes.ContainsKey(methodName)) {
                    Debug.LogError($"Couldn't find Event Function named {methodName} for Object [{obj.GetType()}]");
                    return;
                }
                Type[] expectedTypes = eventFunctionsArgTypes[methodName];
                if (!argsTypes.SequenceEqual(expectedTypes)) {
                    string argNames = string.Join(", ", argsTypes.Select(t => t == null ? "null" : t.Name));
                    Debug.LogError($"Couldn't find Event Function named {methodName} with argument types {argNames} for Object [{obj.GetType()}]");
                    return;
                }
                
                if (!data.ContainsKey(methodName)) data[methodName] = new Dictionary<object, Action<object[]>>();
                var md = (MulticastDelegate)method.toDelegate(obj);
                data[methodName][obj] = args => { md.DynamicInvoke(args); };
            }
        }

        private List<object> awaitToRemove = new();
        public void remove(object obj) {
            awaitToRemove.Add(obj);
        }
        public void _remove(object obj) {
            foreach (var subDict in data.Values.Where(subDict => subDict.ContainsKey(obj))) 
                subDict.Remove(obj);
        }
        
        public void trigger(string eventName, params object[] args) {
            if (!eventFunctionsArgTypes.ContainsKey(eventName)) {
                Debug.LogError("Couldn't find Event Function named " + eventName + "!");
                return;
            }
            
            if (!data.ContainsKey(eventName)) return;
            data[eventName].ForEach(oAndA => {
                if (oAndA.Key is Object uObj && !uObj) {
                    awaitToRemove.Add(uObj);
                    return;
                }
                if (oAndA.Value == null) {
                    Debug.LogError("Pre-registered action is null! This should not happen! Please report this bug to the author of this plugin.");
                    return;
                }
                oAndA.Value(args);
            });
            var self = this;
            awaitToRemove.ForEach(o => {
                self._remove(o);
            });
            awaitToRemove.Clear();
        }
    }
}