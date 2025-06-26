using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Fries.Data;
using Fries.Inspector;
using UnityEngine;

namespace Fries.InsertionEventSys {
    public class InsertionEventSystem : MonoBehaviour {
        private static Dictionary<Type, Dictionary<string, Type[]>> eventList = new();
        public static void declareEvent(Type type, string eventName, Type[] parameters = null) {
            if (!eventList.ContainsKey(type)) eventList[type] = new();
            eventList[type][eventName] = parameters.Nullable();
        }
        
        private static InsertionEventSystem ies;
        public static InsertionEventSystem inst => ies;
        
        private Type[] getTypes(MulticastDelegate delegatee) {
            Type delegateType = delegatee.GetType();
            MethodInfo invokeMethod = delegateType.GetMethod("Invoke");
            Type[] parameterTypes = invokeMethod
                .GetParameters()
                .Select(p => p.ParameterType)
                .ToArray();
            return parameterTypes;
        }
        
        private Dictionary<Type, Dictionary<string, Dictionary<string, MulticastDelegate>>> typeEvent = new();
        public void registerListener(Type type, string eventName, string listenerName, MulticastDelegate listener) {
            if (!eventList.ContainsKey(type)) {
                Debug.LogError($"Given Type {type} has no available events!");
                return;
            }
            if (!eventList[type].ContainsKey(eventName)) {
                Debug.LogError($"Given Type {type} has no event called {eventName}!");
                return;
            }

            Type[] types = getTypes(listener);
            if (types.Length != eventList[type][eventName].Length) {
                Debug.LogError($"Event {eventName} of type {type.ToString()} has {eventList[type][eventName].Length} parameters, but the listener has {types.Length} parameters!");
                return;
            }

            for (int i = 0; i < types.Length; i++) {
                if (eventList[type][eventName][i] == null) 
                    Debug.LogError($"Event {eventName} of type {type.ToString()} has Null parameter {i}");
                if (types[i] == null) 
                    Debug.LogError($"Argument of Event {eventName} of type {type.ToString()} has Null parameter {i}");
                if (eventList[type][eventName][i] == null || types[i].IsAssignableFrom(eventList[type][eventName][i])) continue;
                Debug.LogError($"Event {eventName} of type {type.ToString()} has parameter {i} of type {eventList[type][eventName][i].ToString()}, but the listener has parameter {i} of type {types[i].ToString()}!");
                return;
            }
            
            if (!typeEvent.ContainsKey(type))
                typeEvent[type] = new();
            if (!typeEvent[type].ContainsKey(eventName))
                typeEvent[type][eventName] = new();
            if (typeEvent[type][eventName].ContainsKey(listenerName))
                throw new ArgumentException(
                    $"Event {eventName} of type {type.ToString()} already has a listener called {listenerName}!");
            typeEvent[type][eventName][listenerName] = listener;
        }
        public void unregisterListener(Type type, string eventName, string listenerName) {
            if (!eventList.ContainsKey(type)) {
                Debug.LogError($"Given Type {type} has no available events!");
                return;
            }
            if (!eventList[type].ContainsKey(eventName)) {
                Debug.LogError($"Given Type {type} has no event called {eventName}!");
                return;
            }
            
            if (!typeEvent.ContainsKey(type))
                typeEvent[type] = new();
            if (!typeEvent[type].ContainsKey(eventName))
                typeEvent[type][eventName] = new();
            if (typeEvent[type][eventName].ContainsKey(listenerName))
                typeEvent[type][eventName].Remove(listenerName);
        }
        public void triggerListener(Type type, string eventName, params object[] objects) {
            if (!eventList.ContainsKey(type)) {
                Debug.LogError($"Given Type {type} has no available events!");
                return;
            }
            if (!eventList[type].ContainsKey(eventName)) {
                Debug.LogError($"Given Type {type} has no event called {eventName}!");
                return;
            }
            
            Type[] expectedParams = eventList[type][eventName];
            if (objects.Length != expectedParams.Length) {
                Debug.LogError(
                    $"Event {eventName} of type {type} expects {expectedParams.Length} parameters, " +
                    $"but received {objects.Length}!"
                );
                return;
            }
            
            for (int i = 0; i < objects.Length; i++) {
                var arg = objects[i];
                var expected = expectedParams[i];
                if (!expected.IsInstanceOfType(arg)) {
                    Debug.LogError(
                        $"Event {eventName} parameter {i} expects {expected}, " +
                        $"but got {arg.GetType()}!"
                    );
                    return;
                }
            }
            
            if (!typeEvent.ContainsKey(type))
                typeEvent[type] = new();
            if (!typeEvent[type].ContainsKey(eventName))
                typeEvent[type][eventName] = new();
            foreach (MulticastDelegate listener in typeEvent[type][eventName].Values) {
                try { listener.DynamicInvoke(objects); }
                catch (Exception e) { Debug.LogError($"Catch error in one of the Event Listener: {e}"); }
            }
        }

        private string[] loadAssemblies;

        private void Awake() {
            if (ies) {
                Destroy(gameObject);
                return;
            }
            ies = this;

            loadAssemblies = this.getParam<string[]>(0);
            
            ReflectionUtils.forType(ty => {
                var attrs = ty.GetCustomAttributes(typeof(InsertionEventDeclarer));
                foreach (var attr in attrs) {
                    var declarer = (InsertionEventDeclarer)attr;
                    declareEvent(declarer.type, declarer.eventName, declarer.argsTypes);
                }
            }, typeof(InsertionEventDeclarer), loadAssemblies);
            
            ReflectionUtils.forStaticMethods((mi, de) => {
                InsertionEventListener attr = (InsertionEventListener)mi.GetCustomAttribute(typeof(InsertionEventListener));
                Assembly assembly = mi.DeclaringType.Assembly;
                string fullName = assembly.FullName + "::" + attr.type.Name + "::" + mi.Name;
                try {
                    registerListener(attr.type, attr.eventName, fullName, (MulticastDelegate)de);
                }
                catch (Exception e) {
                    Debug.LogError($"Catch error, check whether you have the valid method signature: void (any) \n {e}");
                }
            }, typeof(InsertionEventListener), BindingFlags.Public | BindingFlags.NonPublic, typeof(void), loadAssemblies);
        }
    }
}